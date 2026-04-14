//
// Mono.MonoBASIC.Tokenizer.cs: The Tokenizer for the MonoBASIC compiler
//
// Author: A Rafael D Teixeira (rafaelteixeirabr@hotmail.com)
//	   
// Based on cs-tokenizer.cs by Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// Copyright (C) 2001 A Rafael D Teixeira
//

namespace Mono.CSharp
{
	using System;
	using System.Text;
	using System.Collections;
	using System.IO;
	using System.Globalization;
	//	using Mono.Languages;
	using Mono.CSharp;
	
	/// <summary>
	///    Tokenizer for MonoBASIC source code. 
	/// </summary>

	// Decimal literals like 32768S are only valid as the operand of unary
	// minus, where VB treats the whole expression as Short.MinValue.  The
	// tokenizer preserves that magnitude so the parser can hand it to the
	// unary-negation resolver instead of rejecting it too early.
	public sealed class SignedMinValueMagnitude
	{
		public readonly TypeCode target_type_code;
		public readonly ulong magnitude;

		public SignedMinValueMagnitude (TypeCode target_type_code, ulong magnitude)
		{
			this.target_type_code = target_type_code;
			this.magnitude = magnitude;
		}
	}
	
	public class Tokenizer : yyParser.yyInput
	{
		SeekableStreamReader reader;
		SourceFile file_name;
		SourceFile ref_name;
		int ref_line = 0;
		int line = 0;
		int col = 1;
		public int current_token = Token.ERROR;
		bool handle_get_set = false;
		bool cant_have_a_type_character = false;

		// Nesting depth of active With blocks.  When greater than zero,
		// a '.' that starts a fresh expression is rewritten to DOT_WITH so
		// the parser can accept VB's implicit receiver form without making
		// '.' a primary-expression starter everywhere.
		public int with_depth = 0;

		public int ExpandedTabsSize = 4; 

		public string location {
			get {
				string det;

				if (current_token == Token.ERROR)
					det = "detail: " + error_details;
				else
					det = "";
				
				return "Line:     "+line+" Col: "+col + "\n" +
				       "VirtLine: "+ref_line +
				       " Token: "+current_token + " " + det;
			}
		}

		public bool properties {
			get {
				return handle_get_set;
			}

			set {
				handle_get_set = value;
			}
                }
		
		//
		// Class variables
		// 
		static Hashtable keywords;
		static NumberStyles styles;
		static NumberFormatInfo csharp_format_info;

		enum IntegerLiteralBase {
			Decimal,
			Hexadecimal,
			Octal
		}

		enum IntegerLiteralType {
			Default,
			Short,
			Integer,
			Long,
			UShort,
			UInteger,
			ULong
		}
		
		//
		// Values for the associated token returned
		//
		StringBuilder number;
		int putback_char = -1;
		Object val;

		sealed class PreprocessorIfFrame
		{
			public readonly bool ParentActive;
			public bool BranchActive;
			public bool AnyTaken;
			public bool SeenElse;

			public PreprocessorIfFrame (bool parentActive, bool branchActive)
			{
				ParentActive = parentActive;
				BranchActive = branchActive;
				AnyTaken = branchActive;
				SeenElse = false;
			}
		}

		sealed class PreprocessorExpressionParser
		{
			enum TokenKind
			{
				End,
				Identifier,
				String,
				Number,
				True,
				False,
				Not,
				And,
				AndAlso,
				Or,
				OrElse,
				Xor,
				OpenParen,
				CloseParen,
				Equals,
				NotEquals,
				Less,
				LessOrEquals,
				Greater,
				GreaterOrEquals,
				Plus,
				Minus,
				Then,
				Error
			}

			readonly string text;
			readonly Hashtable symbols;
			readonly Tokenizer tokenizer;
			readonly Location loc;
			int pos;
			TokenKind token;
			object token_value;
			string token_text;

			public PreprocessorExpressionParser (Tokenizer tokenizer, string text, Hashtable symbols, Location loc)
			{
				this.tokenizer = tokenizer;
				this.text = text;
				this.symbols = symbols;
				this.loc = loc;
				NextToken ();
			}

			public object ParseExpression (bool allow_then)
			{
				object value = ParseXorExpression ();
				if (allow_then && token == TokenKind.Then)
					NextToken ();

				if (token != TokenKind.End)
					Error (30201, "Expression expected.");

				return value;
			}

			void NextToken ()
			{
				SkipWhitespace ();
				token_value = null;
				token_text = null;

				if (pos >= text.Length) {
					token = TokenKind.End;
					return;
				}

				char c = text [pos];
				switch (c) {
				case '(':
					pos++;
					token = TokenKind.OpenParen;
					return;
				case ')':
					pos++;
					token = TokenKind.CloseParen;
					return;
				case '+':
					pos++;
					token = TokenKind.Plus;
					return;
				case '-':
					pos++;
					token = TokenKind.Minus;
					return;
				case '=':
					pos++;
					token = TokenKind.Equals;
					return;
				case '<':
					pos++;
					if (pos < text.Length) {
						if (text [pos] == '=') {
							pos++;
							token = TokenKind.LessOrEquals;
							return;
						}

						if (text [pos] == '>') {
							pos++;
							token = TokenKind.NotEquals;
							return;
						}
					}

					token = TokenKind.Less;
					return;
				case '>':
					pos++;
					if (pos < text.Length && text [pos] == '=') {
						pos++;
						token = TokenKind.GreaterOrEquals;
						return;
					}

					token = TokenKind.Greater;
					return;
				case '"':
					token = TokenKind.String;
					token_value = ReadStringLiteral ();
					return;
				}

				if (Char.IsDigit (c) || (c == '.' && pos + 1 < text.Length && Char.IsDigit (text [pos + 1]))) {
					token = TokenKind.Number;
					token_value = ReadNumberLiteral ();
					return;
				}

				if (Tokenizer.is_identifier_start_character (c)) {
					string ident = ReadIdentifier ();
					token_text = ident;
					switch (ident.ToLower ()) {
					case "true":
						token = TokenKind.True;
						token_value = true;
						return;
					case "false":
						token = TokenKind.False;
						token_value = false;
						return;
					case "not":
						token = TokenKind.Not;
						return;
					case "and":
						token = TokenKind.And;
						return;
					case "andalso":
						token = TokenKind.AndAlso;
						return;
					case "or":
						token = TokenKind.Or;
						return;
					case "orelse":
						token = TokenKind.OrElse;
						return;
					case "xor":
						token = TokenKind.Xor;
						return;
					case "then":
						token = TokenKind.Then;
						return;
					default:
						token = TokenKind.Identifier;
						return;
					}
				}

				token = TokenKind.Error;
				Error (30201, "Expression expected.");
			}

			void SkipWhitespace ()
			{
				while (pos < text.Length && Char.IsWhiteSpace (text [pos]))
					pos++;
			}

			string ReadIdentifier ()
			{
				int start = pos;
				pos++;
				while (pos < text.Length && Tokenizer.is_identifier_part_character (text [pos]))
					pos++;
				return text.Substring (start, pos - start);
			}

			object ReadStringLiteral ()
			{
				StringBuilder sb = new StringBuilder ();
				pos++;
				while (pos < text.Length) {
					char c = text [pos++];
					if (c == '"') {
						if (pos < text.Length && text [pos] == '"') {
							sb.Append ('"');
							pos++;
							continue;
						}

						return sb.ToString ();
					}

					sb.Append (c);
				}

				Error (30201, "Expression expected.");
				return String.Empty;
			}

			object ReadNumberLiteral ()
			{
				int start = pos;
				bool seen_decimal = false;
				while (pos < text.Length) {
					char c = text [pos];
					if (Char.IsDigit (c)) {
						pos++;
						continue;
					}

					if (c == '.' && !seen_decimal) {
						seen_decimal = true;
						pos++;
						continue;
					}

					break;
				}

				return Double.Parse (text.Substring (start, pos - start), NumberStyles.Any, CultureInfo.InvariantCulture);
			}

			object ParseXorExpression ()
			{
				object left = ParseOrExpression ();
				while (token == TokenKind.Xor) {
					NextToken ();
					left = ToBoolean (left) ^ ToBoolean (ParseOrExpression ());
				}

				return left;
			}

			object ParseOrExpression ()
			{
				object left = ParseAndExpression ();
				while (token == TokenKind.Or || token == TokenKind.OrElse) {
					NextToken ();
					left = ToBoolean (left) || ToBoolean (ParseAndExpression ());
				}

				return left;
			}

			object ParseAndExpression ()
			{
				object left = ParseNotExpression ();
				while (token == TokenKind.And || token == TokenKind.AndAlso) {
					NextToken ();
					left = ToBoolean (left) && ToBoolean (ParseNotExpression ());
				}

				return left;
			}

			object ParseNotExpression ()
			{
				if (token == TokenKind.Not) {
					NextToken ();
					return !ToBoolean (ParseNotExpression ());
				}

				return ParseComparisonExpression ();
			}

			object ParseComparisonExpression ()
			{
				object left = ParseUnaryExpression ();
				TokenKind op = token;
				switch (op) {
				case TokenKind.Equals:
				case TokenKind.NotEquals:
				case TokenKind.Less:
				case TokenKind.LessOrEquals:
				case TokenKind.Greater:
				case TokenKind.GreaterOrEquals:
					NextToken ();
					return Compare (left, ParseUnaryExpression (), op);
				default:
					return left;
				}
			}

			object ParseUnaryExpression ()
			{
				if (token == TokenKind.Plus) {
					NextToken ();
					return ToNumber (ParseUnaryExpression ());
				}

				if (token == TokenKind.Minus) {
					NextToken ();
					return -ToNumber (ParseUnaryExpression ());
				}

				return ParsePrimaryExpression ();
			}

			object ParsePrimaryExpression ()
			{
				object value;
				switch (token) {
				case TokenKind.Identifier:
					string name = token_text;
					NextToken ();
					if (symbols.ContainsKey (name))
						return symbols [name];
					return false;
				case TokenKind.String:
				case TokenKind.Number:
				case TokenKind.True:
				case TokenKind.False:
					value = token_value;
					NextToken ();
					return value;
				case TokenKind.OpenParen:
					NextToken ();
					value = ParseXorExpression ();
					if (token != TokenKind.CloseParen)
						Error (30201, "Expression expected.");
					NextToken ();
					return value;
				default:
					Error (30201, "Expression expected.");
					return false;
				}
			}

			bool Compare (object left, object right, TokenKind op)
			{
				if (left is string || right is string) {
					string lhs = ToStringValue (left);
					string rhs = ToStringValue (right);
					int cmp = String.CompareOrdinal (lhs, rhs);
					switch (op) {
					case TokenKind.Equals:
						return cmp == 0;
					case TokenKind.NotEquals:
						return cmp != 0;
					case TokenKind.Less:
						return cmp < 0;
					case TokenKind.LessOrEquals:
						return cmp <= 0;
					case TokenKind.Greater:
						return cmp > 0;
					case TokenKind.GreaterOrEquals:
						return cmp >= 0;
					}
				}

				double lhs_num = ToNumber (left);
				double rhs_num = ToNumber (right);
				switch (op) {
				case TokenKind.Equals:
					return lhs_num == rhs_num;
				case TokenKind.NotEquals:
					return lhs_num != rhs_num;
				case TokenKind.Less:
					return lhs_num < rhs_num;
				case TokenKind.LessOrEquals:
					return lhs_num <= rhs_num;
				case TokenKind.Greater:
					return lhs_num > rhs_num;
				case TokenKind.GreaterOrEquals:
					return lhs_num >= rhs_num;
				default:
					return false;
				}
			}

			static bool ToBoolean (object value)
			{
				if (value is bool)
					return (bool) value;

				if (value is string)
					return ((string) value).Length != 0;

				return ToNumber (value) != 0;
			}

			static double ToNumber (object value)
			{
				if (value is bool)
					return (bool) value ? -1 : 0;

				if (value is double)
					return (double) value;

				if (value is int)
					return (int) value;

				if (value is long)
					return (long) value;

				if (value is string) {
					double parsed;
					if (Double.TryParse ((string) value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
						return parsed;
				}

				return 0;
			}

			static string ToStringValue (object value)
			{
				if (value == null)
					return String.Empty;

				if (value is bool)
					return (bool) value ? "True" : "False";

				if (value is double)
					return ((double) value).ToString (CultureInfo.InvariantCulture);

				return value.ToString ();
			}

			void Error (int code, string message)
			{
				Report.Error (code, loc, message);
				throw new ApplicationException (message);
			}
		}

		Hashtable pp_symbols = new Hashtable (StringComparer.OrdinalIgnoreCase);
		ArrayList pp_if_stack = new ArrayList ();
		int pp_region_depth = 0;
		bool pp_external_source = false;
		
		//
		// Details about the error encoutered by the tokenizer
		//
		string error_details;
		
		public string error {
			get {
				return error_details;
			}
		}

		public int Line {
			get {
				return line;
			}
		}

		public int EffectiveLine {
			get {
				return ref_line;
			}
			set {
				ref_line = value;
			}
		}

		public int Col {
			get {
				return col;
			}
		}
		
		static void initTokens ()
		{
			keywords = new Hashtable ();

			keywords.Add ("addhandler", Token.ADDHANDLER);
			keywords.Add ("addressof", Token.ADDRESSOF);
			keywords.Add ("alias", Token.ALIAS);
			keywords.Add ("and", Token.AND);
			keywords.Add ("andalso", Token.ANDALSO);
			keywords.Add ("ansi", Token.ANSI);
			keywords.Add ("as", Token.AS);
			keywords.Add ("assembly", Token.ASSEMBLY);
			keywords.Add ("auto", Token.AUTO);
			keywords.Add ("binary", Token.BINARY); // Not a VB.NET Keyword 
			keywords.Add ("boolean", Token.BOOLEAN);
			keywords.Add ("byref", Token.BYREF);
			keywords.Add ("byte", Token.BYTE);
			keywords.Add ("byval", Token.BYVAL);
			keywords.Add ("call", Token.CALL);
			keywords.Add ("case", Token.CASE);
			keywords.Add ("catch", Token.CATCH);
			keywords.Add ("cbool", Token.CBOOL);
			keywords.Add ("cbyte", Token.CBYTE);
			keywords.Add ("cchar", Token.CCHAR);
			keywords.Add ("cdate", Token.CDATE);
			keywords.Add ("cdec", Token.CDEC);
			keywords.Add ("cdbl", Token.CDBL);
			keywords.Add ("char", Token.CHAR);
			keywords.Add ("cint", Token.CINT);
			keywords.Add ("class", Token.CLASS);
			keywords.Add ("clng", Token.CLNG);
			keywords.Add ("cobj", Token.COBJ);
			keywords.Add ("compare", Token.COMPARE); // Not a VB.NET Keyword
			keywords.Add ("const", Token.CONST);
			keywords.Add ("cshort", Token.CSHORT);
			keywords.Add ("csng", Token.CSNG);
			keywords.Add ("cstr", Token.CSTR);
			keywords.Add ("ctype", Token.CTYPE);
			keywords.Add ("date", Token.DATE);
			keywords.Add ("decimal", Token.DECIMAL);
			keywords.Add ("declare", Token.DECLARE);
			keywords.Add ("default", Token.DEFAULT);
			keywords.Add ("delegate", Token.DELEGATE);
			keywords.Add ("dim", Token.DIM);
			keywords.Add ("directcast", Token.DIRECTCAST);
			keywords.Add ("trycast", Token.TRYCAST);
			keywords.Add ("do", Token.DO);
			keywords.Add ("double", Token.DOUBLE);
			keywords.Add ("each", Token.EACH);
			keywords.Add ("else", Token.ELSE);
			keywords.Add ("elseif", Token.ELSEIF);
			keywords.Add ("end", Token.END);
			keywords.Add ("endif", Token.ENDIF); // An unused VB.NET keyword
			keywords.Add ("enum", Token.ENUM);
			keywords.Add ("erase", Token.ERASE);
			keywords.Add ("error", Token.ERROR);
			keywords.Add ("event", Token.EVENT);
			keywords.Add ("exit", Token.EXIT);
			keywords.Add ("explicit", Token.EXPLICIT); // Not a VB.NET keyword 
			keywords.Add ("false", Token.FALSE);
			keywords.Add ("finally", Token.FINALLY);
			keywords.Add ("for", Token.FOR);
			keywords.Add ("friend", Token.FRIEND);
			keywords.Add ("function", Token.FUNCTION);
			keywords.Add ("get", Token.GET);
			keywords.Add ("gettype", Token.GETTYPE);
			keywords.Add ("gosub", Token.GOSUB); // An unused VB.NET keyword 
			keywords.Add ("goto", Token.GOTO);
			keywords.Add ("handles", Token.HANDLES);
			keywords.Add ("if", Token.IF);
			keywords.Add ("implements", Token.IMPLEMENTS);
			keywords.Add ("imports", Token.IMPORTS);
			keywords.Add ("in", Token.IN);
			keywords.Add ("inherits", Token.INHERITS);
			keywords.Add ("integer", Token.INTEGER);
			keywords.Add ("interface", Token.INTERFACE);
			keywords.Add ("is", Token.IS);
			keywords.Add ("isnot", Token.ISNOT);
			keywords.Add ("let", Token.LET ); // An unused VB.NET keyword
			keywords.Add ("lib", Token.LIB );
			keywords.Add ("like", Token.LIKE );
			keywords.Add ("long", Token.LONG);
			keywords.Add ("loop", Token.LOOP);
			keywords.Add ("me", Token.ME);
			keywords.Add ("mod", Token.MOD);
			keywords.Add ("module", Token.MODULE);
			keywords.Add ("mustinherit", Token.MUSTINHERIT);
			keywords.Add ("mustoverride", Token.MUSTOVERRIDE);
			keywords.Add ("mybase", Token.MYBASE);
			keywords.Add ("myclass", Token.MYCLASS);
			keywords.Add ("namespace", Token.NAMESPACE);
			keywords.Add ("new", Token.NEW);
			keywords.Add ("next", Token.NEXT);
			keywords.Add ("not", Token.NOT);
			keywords.Add ("nothing", Token.NOTHING);
			keywords.Add ("notinheritable", Token.NOTINHERITABLE);
			keywords.Add ("notoverridable", Token.NOTOVERRIDABLE);
			keywords.Add ("object", Token.OBJECT);
			keywords.Add ("of", Token.OF);
			keywords.Add ("off", Token.OFF); // Not a VB.NET Keyword
			keywords.Add ("operator", Token.OPERATOR);
			keywords.Add ("narrowing", Token.NARROWING);
			keywords.Add ("on", Token.ON);
			keywords.Add ("option", Token.OPTION);
			keywords.Add ("optional", Token.OPTIONAL);
			keywords.Add ("partial", Token.PARTIAL);
			keywords.Add ("or", Token.OR);
			keywords.Add ("orelse", Token.ORELSE);
			keywords.Add ("overloads", Token.OVERLOADS);
			keywords.Add ("overridable", Token.OVERRIDABLE);
			keywords.Add ("overrides", Token.OVERRIDES);
			keywords.Add ("paramarray", Token.PARAM_ARRAY);
			keywords.Add ("preserve", Token.PRESERVE);
			keywords.Add ("private", Token.PRIVATE);
			keywords.Add ("property", Token.PROPERTY);
			keywords.Add ("protected", Token.PROTECTED);
			keywords.Add ("public", Token.PUBLIC);
			keywords.Add ("raiseevent", Token.RAISEEVENT);
			keywords.Add ("readonly", Token.READONLY);
			keywords.Add ("redim", Token.REDIM);
			keywords.Add ("rem", Token.REM);
			keywords.Add ("removehandler", Token.REMOVEHANDLER);
			keywords.Add ("resume", Token.RESUME);
			keywords.Add ("return", Token.RETURN);
			keywords.Add ("select", Token.SELECT);
			keywords.Add ("set", Token.SET);
			keywords.Add ("shadows", Token.SHADOWS);
			keywords.Add ("shared", Token.SHARED);
			keywords.Add ("short", Token.SHORT);
			keywords.Add ("single", Token.SINGLE);
			keywords.Add ("sizeof", Token.SIZEOF); // Not a VB.NET Keyword 
			keywords.Add ("static", Token.STATIC);
			keywords.Add ("step", Token.STEP);
			keywords.Add ("stop", Token.STOP);
			keywords.Add ("strict", Token.STRICT); // Not a VB.NET Keyword 
			keywords.Add ("string", Token.STRING);
			keywords.Add ("structure", Token.STRUCTURE);
			keywords.Add ("sub", Token.SUB);
			keywords.Add ("synclock", Token.SYNCLOCK);
			keywords.Add ("text", Token.TEXT); // Not a VB.NET Keyword
			keywords.Add ("then", Token.THEN);
			keywords.Add ("throw", Token.THROW);
			keywords.Add ("to", Token.TO);
			keywords.Add ("true", Token.TRUE);
			keywords.Add ("try", Token.TRY);
			keywords.Add ("typeof", Token.TYPEOF);
			keywords.Add ("sbyte", Token.SBYTE);
			keywords.Add ("uinteger", Token.UINTEGER);
			keywords.Add ("ulong", Token.ULONG);
			keywords.Add ("ushort", Token.USHORT);
			keywords.Add ("unicode", Token.UNICODE);
			keywords.Add ("until", Token.UNTIL);
			keywords.Add ("using", Token.USING);
			keywords.Add ("variant", Token.VARIANT); // An unused VB.NET keyword
			keywords.Add ("wend", Token.WEND); // An unused VB.NET keyword
			keywords.Add ("when", Token.WHEN);
			keywords.Add ("while", Token.WHILE);
			keywords.Add ("widening", Token.WIDENING);
			keywords.Add ("with", Token.WITH);
			keywords.Add ("withevents", Token.WITHEVENTS);
			keywords.Add ("writeonly", Token.WRITEONLY);
			keywords.Add ("xor", Token.XOR);

			/*

			if (Parser.UseExtendedSyntax){
				keywords.Add ("yield", Token.YIELD);
			}
			*/


		}

		static Tokenizer ()
		{
			initTokens ();
			csharp_format_info = new NumberFormatInfo ();
			csharp_format_info.CurrencyDecimalSeparator = ".";
			styles = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint;
		}

		public Tokenizer (SeekableStreamReader input, SourceFile fname, ArrayList defines)
		{
			this.ref_name = fname;
			this.file_name = fname;

			reader = input;

			SeedPreprocessorSymbols (defines);

			// putback an EOL at the beginning of a stream. This is a convenience that 
			// allows pre-processor directives to be added to the beginning of a vb file.
			putback('\n');
		}

		void SeedPreprocessorSymbols (ArrayList defines)
		{
			if (defines == null)
				return;

			foreach (string raw_define in defines) {
				if (raw_define == null)
					continue;

				string define = raw_define.Trim ();
				if (define.Length == 0)
					continue;

				int equals = define.IndexOf ('=');
				if (equals < 0) {
					pp_symbols [define] = true;
					continue;
				}

				string name = define.Substring (0, equals).Trim ();
				string value = define.Substring (equals + 1).Trim ();
				if (!IsValidIdentifier (name))
					continue;

				pp_symbols [name] = ParseDefinedConstantValue (value);
			}
		}

		object ParseDefinedConstantValue (string text)
		{
			if (text == null || text.Length == 0)
				return true;

			if (text.Length >= 2 && text [0] == '"' && text [text.Length - 1] == '"') {
				string inner = text.Substring (1, text.Length - 2);
				inner = inner.Replace ("\\\"", "\"");
				inner = inner.Replace ("\"\"", "\"");
				return inner;
			}

			if (String.Compare (text, "True", true, CultureInfo.InvariantCulture) == 0)
				return true;

			if (String.Compare (text, "False", true, CultureInfo.InvariantCulture) == 0)
				return false;

			double parsed;
			if (Double.TryParse (text, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
				return parsed;

			return text;
		}

		bool is_keyword (string name)
		{
			bool res;
			name = name.ToLower();

			res = keywords.Contains(name);
			if ((name == "GET" || name == "SET") && handle_get_set == false)
				return false;
			return res;
		}

		int getKeyword (string name)
		{
			return (int) (keywords [name.ToLower()]);
		}
		
		public Location Location {
			get {
				Location.Push (ref_name);
				return new Location (ref_line);
			}
		}
		
		public bool PropertyParsing {
			get {
				return handle_get_set;
			}

			set {
				handle_get_set = value;
			}
                }
                		
		static bool is_identifier_start_character (char c)
		{
			return Char.IsLetter (c) || c == '_' ;
		}

		static bool is_identifier_part_character (char c)
		{
			return (Char.IsLetter (c) || Char.IsDigit (c) || c == '_');
		}

		public static bool IsValidIdentifier (string s)
		{
			if (s == null || s.Length == 0)
				return false;
			
			if (!is_identifier_start_character (s [0]))
				return false;
			
			for (int i = 1; i < s.Length; i ++)
				if (! is_identifier_part_character (s [i]))
					return false;
			
			return true;
		}

		int is_punct (char c, ref bool doread)
		{
			int d;
			int t;

			doread = false;
			
			error_details = c.ToString();
			
			d = peekChar ();
			
			switch (c){
			case '[':
				return Token.OPEN_BRACKET;
			case ']':
				return Token.CLOSE_BRACKET;
			case '{':
				return Token.OPEN_BRACE;
			case '}':
				return Token.CLOSE_BRACE;				
			case '(':
				return Token.OPEN_PARENS;
			case ')':
				return Token.CLOSE_PARENS;
			case ',':
				return Token.COMMA;
			case '?':
				return Token.INTERR;
			case '!':
				if (is_identifier_start_character((char)d) || cant_have_a_type_character)
					return Token.EXCLAMATION;
				return Token.SINGLETYPECHAR;
			case '$':
				if (cant_have_a_type_character)
					return Token.ERROR;
				return Token.DOLAR_SIGN;
			case '@':
				if (cant_have_a_type_character)
					return Token.ERROR;
				return Token.AT_SIGN;
			case '%':
				if (cant_have_a_type_character)
					return Token.ERROR;
				return Token.PERCENT;
			case '#':
				if(tokens_seen)
				{
					if (cant_have_a_type_character) 
						return ParseDateLiteral();
					else
						return Token.NUMBER_SIGN;
				}
				else 
				{
					tokens_seen = true;
					return Token.HASH;
				} 
			case '&':
				if (!cant_have_a_type_character)
					return Token.LONGTYPECHAR;
				t = handle_integer_literal_in_other_bases(d);
				if (t == Token.NONE) {
					t = Token.OP_CONCAT;
				}
				return t;			
			}

			if (c == '+'){
				if (d == '+')
					t = Token.OP_INC;
				else 
					return Token.PLUS;
				doread = true;
				return t;
			}
			if (c == '-'){
				return Token.MINUS;
			}

			if (c == '='){
				return Token.ASSIGN;
			}

			if (c == '*'){
				return Token.STAR;
			}

			if (c == '/'){
				return Token.DIV;
			}

			if (c == '\\'){
				return Token.OP_IDIV;
			}

			if (c == '^'){
				return Token.OP_EXP;
			}

			if (c == '<'){
				if (d == '>')
				{
					doread = true;
					return Token.OP_NE;
				}
				if (d == '='){
					doread = true;
					return Token.OP_LE;
				}
				if (d == '<')
				{
					doread = true;
					return Token.OP_SHIFT_LEFT;
				}
				return Token.OP_LT;
			}

			if (c == '>'){
				if (d == '='){
					doread = true;
					return Token.OP_GE;
				}
				if (d == '>')
				{
					doread = true;
					return Token.OP_SHIFT_RIGHT;
				}
				return Token.OP_GT;
			}
			
			if (c == ':'){
				if (d == '='){
					doread = true;
					return Token.ATTR_ASSIGN;
				}
				return Token.COLON;
			}			
			
			return Token.ERROR;
		}

		bool decimal_digits (int c)
		{
			int d;
			bool seen_digits = false;
			
			if (c != -1)
				number.Append ((char) c);
			
			while ((d = peekChar ()) != -1){
				if (Char.IsDigit ((char)d)){
					number.Append ((char) d);
					getChar ();
					seen_digits = true;
				} else
					break;
			}
			return seen_digits;
		}

		
		int real_type_suffix (int c)
		{
			int t;
			
			switch (c){
			case 'F': case 'f':
			case '!':
				t =  Token.LITERAL_SINGLE;
				break;
			case 'R': case 'r':
			case '#':
				t = Token.LITERAL_DOUBLE;
				break;
			case 'D': case 'd':
			case '@':
				 t= Token.LITERAL_DECIMAL;
				break;
			default:
				return Token.NONE;
			}
			getChar ();
			return t;
		}

		static ulong ParseOctalDigits (string digits)
		{
			ulong value = 0;

			foreach (char digit in digits) {
				checked {
					value = value * 8 + (ulong) (digit - '0');
				}
			}

			return value;
		}

		static ulong ParseUnsignedIntegerLiteral (string digits, IntegerLiteralBase literalBase)
		{
			switch (literalBase) {
			case IntegerLiteralBase.Decimal:
				return UInt64.Parse (digits, NumberStyles.None, CultureInfo.InvariantCulture);
			case IntegerLiteralBase.Hexadecimal:
				return UInt64.Parse (digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			case IntegerLiteralBase.Octal:
				return ParseOctalDigits (digits);
			default:
				throw new InvalidOperationException ();
			}
		}

		static object ParseTypedIntegerLiteral (string digits, IntegerLiteralBase literalBase, IntegerLiteralType literalType)
		{
			ulong raw = ParseUnsignedIntegerLiteral (digits, literalBase);

			switch (literalType) {
			case IntegerLiteralType.Short:
				if (literalBase == IntegerLiteralBase.Decimal) {
					if (raw > (ulong) Int16.MaxValue) {
						if (raw == ((ulong) Int16.MaxValue) + 1)
							return new SignedMinValueMagnitude (TypeCode.Int16, raw);
						throw new OverflowException ("Integer literal is too large for Short");
					}
					return (short) raw;
				}

				if (raw > UInt16.MaxValue)
					throw new OverflowException ("Integer literal does not fit in a 16-bit Short bit-pattern");
				return unchecked ((short) ((ushort) raw));

			case IntegerLiteralType.Integer:
				if (literalBase == IntegerLiteralBase.Decimal) {
					if (raw > (ulong) Int32.MaxValue) {
						if (raw == ((ulong) Int32.MaxValue) + 1)
							return new SignedMinValueMagnitude (TypeCode.Int32, raw);
						throw new OverflowException ("Integer literal is too large for Integer");
					}
					return (int) raw;
				}

				if (raw > UInt32.MaxValue)
					throw new OverflowException ("Integer literal does not fit in a 32-bit Integer bit-pattern");
				return unchecked ((int) ((uint) raw));

			case IntegerLiteralType.Long:
				if (literalBase == IntegerLiteralBase.Decimal) {
					if (raw > Int64.MaxValue) {
						if (raw == ((ulong) Int64.MaxValue) + 1)
							return new SignedMinValueMagnitude (TypeCode.Int64, raw);
						throw new OverflowException ("Integer literal is too large for Long");
					}
					return (long) raw;
				}

				return unchecked ((long) raw);

			case IntegerLiteralType.UShort:
				if (raw > UInt16.MaxValue)
					throw new OverflowException ("Integer literal is too large for UShort");
				return (ushort) raw;

			case IntegerLiteralType.UInteger:
				if (raw > UInt32.MaxValue)
					throw new OverflowException ("Integer literal is too large for UInteger");
				return (uint) raw;

			case IntegerLiteralType.ULong:
				return raw;

			default:
				throw new InvalidOperationException ();
			}
		}

		static object ParseDefaultIntegerLiteral (string digits, IntegerLiteralBase literalBase)
		{
			ulong raw = ParseUnsignedIntegerLiteral (digits, literalBase);

			if (literalBase == IntegerLiteralBase.Decimal) {
				if (raw <= Int32.MaxValue)
					return (int) raw;

				if (raw <= Int64.MaxValue)
					return (long) raw;

				throw new OverflowException ("Integer literal is too large for Integer or Long");
			}

			if (raw <= UInt32.MaxValue)
				return unchecked ((int) ((uint) raw));

			return unchecked ((long) raw);
		}

		int integer_type_suffix (int c, string digits, IntegerLiteralBase literalBase)
		{
			IntegerLiteralType literalType = IntegerLiteralType.Default;

			switch (c) {
			case 'S': case 's':
				getChar ();
				literalType = IntegerLiteralType.Short;
				break;
			case 'I': case 'i':
				getChar ();
				literalType = IntegerLiteralType.Integer;
				break;
			case 'L': case 'l':
				getChar ();
				literalType = IntegerLiteralType.Long;
				break;
			case 'U': case 'u':
				getChar ();
				switch (peekChar ()) {
				case 'S': case 's':
					getChar ();
					literalType = IntegerLiteralType.UShort;
					break;
				case 'I': case 'i':
					getChar ();
					literalType = IntegerLiteralType.UInteger;
					break;
				case 'L': case 'l':
					getChar ();
					literalType = IntegerLiteralType.ULong;
					break;
				default:
					val = "'U' integer suffix must be followed by S, I, or L";
					return Token.ERROR;
				}
				break;
			}

			try {
				if (literalType == IntegerLiteralType.Default)
					val = ParseDefaultIntegerLiteral (digits, literalBase);
				else
					val = ParseTypedIntegerLiteral (digits, literalBase, literalType);
				return Token.LITERAL_INTEGER;
			} catch (Exception e) {
				val = e.Message;
				return Token.ERROR;
			}
		}
		
		int adjust_real (int t)
		{
			string s = number.ToString ();

			switch (t){
			case Token.LITERAL_DECIMAL:
				val = new System.Decimal ();
				val = System.Decimal.Parse (
					s, styles, csharp_format_info);
				break;
			case Token.LITERAL_DOUBLE:
				val = new System.Double ();
				val = System.Double.Parse (
					s, styles, csharp_format_info);
				break;
			case Token.LITERAL_SINGLE:
				val = new System.Double ();
				val = (float) System.Double.Parse (
					s, styles, csharp_format_info);
				break;

			case Token.NONE:
				val = new System.Double ();
				val = System.Double.Parse (
					s, styles, csharp_format_info);
				t = Token.LITERAL_DOUBLE;
				break;
			}
			return t;
		}

		string hex_digits ()
		{
			StringBuilder hexNumber = new StringBuilder ();
			
			int d;

			while ((d = peekChar ()) != -1){
				char e = Char.ToUpper ((char) d);
				
				if (Char.IsDigit (e) || (e >= 'A' && e <= 'F')){
					hexNumber.Append (e);
					getChar ();
				} else
					break;
			}
			return hexNumber.ToString ();
		}

		string octal_digits ()
		{
			StringBuilder octalNumber = new StringBuilder ();
			
			int d;

			while ((d = peekChar ()) != -1){
				char e = (char)d;			
				if (Char.IsDigit (e) && (e < '8')){
					octalNumber.Append (e);
					getChar ();
				} else
					break;
			}
			
			return octalNumber.ToString ();
		}

		int handle_integer_literal_in_other_bases(int peek)
		{
			if (peek == 'h' || peek == 'H'){
				string digits;

				getChar ();
				digits = hex_digits ();
				return integer_type_suffix (peekChar (), digits, IntegerLiteralBase.Hexadecimal);
			}
			
			if (peek == 'o' || peek == 'O'){
				string digits;

				getChar ();
				digits = octal_digits ();
				return integer_type_suffix (peekChar (), digits, IntegerLiteralBase.Octal);
			}
			
			return Token.NONE;
		}
		
		//
		// Invoked if we know we have .digits or digits
		//
		int is_number (int c)
		{
			bool is_real = false;
			number = new StringBuilder ();
			int type;

			number.Length = 0;

			if (Char.IsDigit ((char)c)){
				decimal_digits (c);
				c = peekChar ();
			}

			//
			// We need to handle the case of
			// "1.1" vs "1.ToString()" (LITERAL_SINGLE vs NUMBER DOT IDENTIFIER)
			//
			if (c == '.'){
				if (decimal_digits (getChar())){
					is_real = true;
					c = peekChar ();
				} else {
					putback ('.');
					number.Length -= 1;
					return integer_type_suffix ('.', number.ToString (), IntegerLiteralBase.Decimal);
				}
			}
			
			if (c == 'e' || c == 'E'){
				is_real = true;
				number.Append ("e");
				getChar ();
				
				c = peekChar ();
				if (c == '+'){
					number.Append ((char) c);
					getChar ();
					c = peekChar ();
				} else if (c == '-'){
					number.Append ((char) c);
					getChar ();
					c = peekChar ();
				}
				decimal_digits (-1);
				c = peekChar ();
			}

			type = real_type_suffix (c);
			if (type == Token.NONE && !is_real){
				return integer_type_suffix (c, number.ToString (), IntegerLiteralBase.Decimal);
			}
			
			return adjust_real (type);
		}
			
		int getChar ()
		{
			if (putback_char != -1){
				int x = putback_char;
				putback_char = -1;

				return x;
			}
			return reader.Read ();
		}

		int peekChar ()
		{
			if (putback_char != -1)
				return putback_char;
			return reader.Peek ();
		}

		void putback (int c)
		{
			if (putback_char != -1)
				throw new Exception ("This should not happen putback on putback");
			putback_char = c;
		}

		public bool advance ()
		{
			return current_token != Token.EOF ;
		}

		public Object Value {
			get {
				return val;
			}
		}

		public Object value ()
		{
			return val;
		}

		private bool IsEOL(int currentChar)
		{
			bool retVal;
			
			if (currentChar ==  0x0D) {
				if (peekChar() ==  0x0A) // if it is a CR-LF pair consume LF also
					getChar();

				retVal = true;
			}
			else {
				retVal = (currentChar ==  -1 || currentChar ==  0x0A || currentChar ==  0x2028 || currentChar ==  0x2029);
			}

			if(retVal) {
				nextLine();
			}

			return retVal;
		}

		private int DropComments()		
		{
			int d;
			while (!IsEOL(d = getChar ()))
				col++;

			return Token.EOL;
		}	
			
		// Single-slot token buffer used to merge the two-token sequence
		// "OPEN_PARENS OF" into a single OPEN_PARENS_OF token. The VB
		// parser needs to distinguish "foo(" (method call or array index)
		// from "foo(Of" (generic type-argument list) with LALR(1) lookahead;
		// merging the two tokens into one at the lexer level resolves the
		// ambiguity cleanly.
		int pending_token = 0;
		object pending_val = null;

		bool IsPreprocessorSkipping ()
		{
			if (pp_if_stack.Count == 0)
				return false;

			return !((PreprocessorIfFrame) pp_if_stack [pp_if_stack.Count - 1]).BranchActive;
		}

		string ReadPhysicalDirectiveLine ()
		{
			StringBuilder sb = new StringBuilder ();
			int c;
			while (!IsEOL (c = getChar ())) {
				sb.Append ((char) c);
				col++;
			}

			return sb.ToString ();
		}

		static string StripDirectiveComment (string line)
		{
			bool in_string = false;
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < line.Length; ++i) {
				char c = line [i];
				if (c == '"') {
					if (in_string && i + 1 < line.Length && line [i + 1] == '"') {
						sb.Append ("\"\"");
						i++;
						continue;
					}

					in_string = !in_string;
					sb.Append (c);
					continue;
				}

				if (!in_string && c == '\'')
					break;

				sb.Append (c);
			}

			return sb.ToString ();
		}

		string ReadDirectiveText ()
		{
			StringBuilder sb = new StringBuilder ();
			for (;;) {
				string physical = StripDirectiveComment (ReadPhysicalDirectiveLine ());
				string trimmed = physical.TrimEnd ();
				if (trimmed.EndsWith ("_")) {
					sb.Append (trimmed.Substring (0, trimmed.Length - 1));
					sb.Append (' ');
					continue;
				}

				sb.Append (physical);
				return sb.ToString ().Trim ();
			}
		}

		static void SkipWhitespace (string text, ref int pos)
		{
			while (pos < text.Length && Char.IsWhiteSpace (text [pos]))
				pos++;
		}

		static string ReadDirectiveKeyword (string text, ref int pos)
		{
			SkipWhitespace (text, ref pos);
			int start = pos;
			while (pos < text.Length && is_identifier_part_character (text [pos]))
				pos++;

			if (start == pos)
				return null;

			return text.Substring (start, pos - start);
		}

		static bool TryReadDirectiveStringLiteral (string text, ref int pos, out string value)
		{
			value = null;
			SkipWhitespace (text, ref pos);
			if (pos >= text.Length || text [pos] != '"')
				return false;

			StringBuilder sb = new StringBuilder ();
			pos++;
			while (pos < text.Length) {
				char c = text [pos++];
				if (c == '"') {
					if (pos < text.Length && text [pos] == '"') {
						sb.Append ('"');
						pos++;
						continue;
					}

					value = sb.ToString ();
					return true;
				}

				sb.Append (c);
			}

			return false;
		}

		static bool TryReadDirectiveIntegerLiteral (string text, ref int pos, out int value)
		{
			value = 0;
			SkipWhitespace (text, ref pos);
			int start = pos;
			if (pos < text.Length && (text [pos] == '+' || text [pos] == '-'))
				pos++;

			while (pos < text.Length && Char.IsDigit (text [pos]))
				pos++;

			if (start == pos || (start + 1 == pos && (text [start] == '+' || text [start] == '-')))
				return false;

			return Int32.TryParse (text.Substring (start, pos - start), out value);
		}

		bool HandlePreprocessorDirective ()
		{
			Location directive_location = Location;
			string text = ReadDirectiveText ();
			int pos = 0;
			string keyword = ReadDirectiveKeyword (text, ref pos);
			if (keyword == null) {
				Report.Error (30203, directive_location, "Identifier expected.");
				return true;
			}

			string lower_keyword = keyword.ToLower ();
			switch (lower_keyword) {
			case "if":
				HandlePreprocessorIf (directive_location, text.Substring (pos));
				return true;
			case "elseif":
				HandlePreprocessorElseIf (directive_location, text.Substring (pos));
				return true;
			case "else":
				HandlePreprocessorElse (directive_location, text.Substring (pos));
				return true;
			case "endif":
				SkipWhitespace (text, ref pos);
				if (pos != text.Length)
					Report.Error (29999, directive_location, "Unrecognized Pre-Processor statement");
				HandlePreprocessorEndIf (directive_location);
				return true;
			}

			if (lower_keyword == "end") {
				int end_pos = pos;
				string end_keyword = ReadDirectiveKeyword (text, ref end_pos);
				if (end_keyword != null && end_keyword.ToLower () == "if") {
					HandlePreprocessorEndDirective (directive_location, text, ref pos);
					return true;
				}
			}

			if (IsPreprocessorSkipping ())
				return true;

			switch (lower_keyword) {
			case "end":
				HandlePreprocessorEndDirective (directive_location, text, ref pos);
				return true;
			case "const":
				HandlePreprocessorConst (directive_location, text, ref pos);
				return true;
			case "region":
				HandlePreprocessorRegion (directive_location, text, ref pos);
				return true;
			case "externalsource":
				HandleExternalSourceDirective (directive_location, text.Substring (pos));
				return true;
			default:
				Report.Error (29999, directive_location, "Unrecognized Pre-Processor statement");
				return true;
			}
		}

		void HandlePreprocessorIf (Location loc, string expression_text)
		{
			bool parent_active = pp_if_stack.Count == 0 || ((PreprocessorIfFrame) pp_if_stack [pp_if_stack.Count - 1]).BranchActive;
			bool branch_active = false;
			try {
				object condition = new PreprocessorExpressionParser (this, expression_text, pp_symbols, loc).ParseExpression (true);
				branch_active = parent_active && ToPreprocessorBoolean (condition);
			} catch (ApplicationException) {
				branch_active = false;
			}

			pp_if_stack.Add (new PreprocessorIfFrame (parent_active, branch_active));
		}

		void HandlePreprocessorRegion (Location loc, string text, ref int pos)
		{
			string region_name;
			if (!TryReadDirectiveStringLiteral (text, ref pos, out region_name)) {
				Report.Error (30217, loc, "String constant expected.");
				return;
			}

			SkipWhitespace (text, ref pos);
			if (pos != text.Length) {
				Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
				return;
			}

			pp_region_depth++;
		}

		void HandlePreprocessorElseIf (Location loc, string expression_text)
		{
			if (pp_if_stack.Count == 0) {
				Report.Error (30014, loc, "#ElseIf must be preceded by a matching #If or #ElseIf");
				return;
			}

			PreprocessorIfFrame frame = (PreprocessorIfFrame) pp_if_stack [pp_if_stack.Count - 1];
			if (frame.SeenElse) {
				Report.Error (32030, loc, "#ElseIf cannot follow #Else as part of #If block");
				frame.BranchActive = false;
				return;
			}

			if (!frame.ParentActive || frame.AnyTaken) {
				frame.BranchActive = false;
				return;
			}

			try {
				object condition = new PreprocessorExpressionParser (this, expression_text, pp_symbols, loc).ParseExpression (true);
				frame.BranchActive = ToPreprocessorBoolean (condition);
				frame.AnyTaken = frame.BranchActive;
			} catch (ApplicationException) {
				frame.BranchActive = false;
			}
		}

		void HandlePreprocessorElse (Location loc, string trailing_text)
		{
			if (pp_if_stack.Count == 0) {
				Report.Error (30028, loc, "#Else must be preceded by a matching #If or #ElseIf");
				return;
			}

			if (trailing_text.Trim ().Length != 0)
				Report.Error (29999, loc, "Unrecognized Pre-Processor statement");

			PreprocessorIfFrame frame = (PreprocessorIfFrame) pp_if_stack [pp_if_stack.Count - 1];
			if (frame.SeenElse) {
				Report.Error (30028, loc, "#Else must be preceded by a matching #If or #ElseIf");
				frame.BranchActive = false;
				return;
			}

			frame.SeenElse = true;
			frame.BranchActive = frame.ParentActive && !frame.AnyTaken;
			frame.AnyTaken = frame.AnyTaken || frame.BranchActive;
		}

		void HandlePreprocessorEndIf (Location loc)
		{
			if (pp_if_stack.Count == 0) {
				Report.Error (30013, loc, "#ElseIf, #Else or #End If must be preceded by a matching #If");
				return;
			}

			pp_if_stack.RemoveAt (pp_if_stack.Count - 1);
		}

		void HandlePreprocessorEndDirective (Location loc, string text, ref int pos)
		{
			string keyword = ReadDirectiveKeyword (text, ref pos);
			if (keyword == null) {
				Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
				return;
			}

			switch (keyword.ToLower ()) {
			case "if":
				SkipWhitespace (text, ref pos);
				if (pos != text.Length) {
					Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
					return;
				}
				HandlePreprocessorEndIf (loc);
				return;
			case "region":
				SkipWhitespace (text, ref pos);
				if (pos != text.Length) {
					Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
					return;
				}
				if (pp_region_depth > 0)
					pp_region_depth--;
				else
					Report.Error (30205, loc, "'#End Region' must be preceded  by a matching '#Region'");
				return;
			case "externalsource":
				if (!pp_external_source)
					Report.Error (30578, loc, "'#End ExternalSource' must be preceded by a matching '#ExternalSource'");
				else {
					pp_external_source = false;
					ref_name = file_name;
					ref_line = line;
					Location.Push (ref_name);
				}

				SkipWhitespace (text, ref pos);
				if (pos != text.Length)
					Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
				return;
			default:
				Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
				return;
			}
		}

		void HandleExternalSourceDirective (Location loc, string trailing_text)
		{
			if (pp_external_source) {
				Report.Error (30580, loc, "#ExternalSource directives may not be nested");
				return;
			}

			int pos = 0;
			SkipWhitespace (trailing_text, ref pos);
			if (pos >= trailing_text.Length || trailing_text [pos] != '(') {
				Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
				return;
			}

			pos++;

			string mapped_name;
			if (!TryReadDirectiveStringLiteral (trailing_text, ref pos, out mapped_name)) {
				Report.Error (30217, loc, "String constant expected.");
				return;
			}

			SkipWhitespace (trailing_text, ref pos);
			if (pos >= trailing_text.Length || trailing_text [pos] != ',') {
				Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
				return;
			}

			pos++;

			int mapped_line;
			if (!TryReadDirectiveIntegerLiteral (trailing_text, ref pos, out mapped_line)) {
				Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
				return;
			}

			SkipWhitespace (trailing_text, ref pos);
			if (pos >= trailing_text.Length || trailing_text [pos] != ')') {
				Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
				return;
			}

			pos++;
			SkipWhitespace (trailing_text, ref pos);
			if (pos != trailing_text.Length) {
				Report.Error (29999, loc, "Unrecognized Pre-Processor statement");
				return;
			}

			ref_name = Location.LookupFile (mapped_name);
			file_name.HasLineDirective = true;
			ref_name.HasLineDirective = true;
			// ReadDirectiveText consumes the directive line's terminating EOL
			// before we process the directive, but the next source line must
			// still map to the directive's declared starting line.
			ref_line = mapped_line;
			Location.Push (ref_name);
			pp_external_source = true;
		}

		void HandlePreprocessorConst (Location loc, string text, ref int pos)
		{
			string name = ReadDirectiveKeyword (text, ref pos);
			if (name == null) {
				Report.Error (30203, loc, "Identifier expected.");
				return;
			}

			SkipWhitespace (text, ref pos);
			if (pos >= text.Length || text [pos] != '=') {
				Report.Error (30249, loc, "'=' expected.");
				return;
			}

			pos++;
			if (IsPreprocessorSkipping ())
				return;

			try {
				object value = new PreprocessorExpressionParser (this, text.Substring (pos), pp_symbols, loc).ParseExpression (false);
				pp_symbols [name] = value;
			} catch (ApplicationException) {
			}
		}

		static bool ToPreprocessorBoolean (object value)
		{
			if (value is bool)
				return (bool) value;

			if (value is string)
				return ((string) value).Length != 0;

			if (value is double)
				return (double) value != 0;

			if (value is int)
				return (int) value != 0;

			if (value is long)
				return (long) value != 0;

			return value != null;
		}

		void SkipInactiveLine ()
		{
			while (current_token != Token.EOL &&
			       current_token != Token.EOF &&
			       current_token != 0) {
				current_token = xtoken ();
				if (current_token == Token.REM)
					current_token = DropComments ();
			}
		}

		public int token ()
		{
			if (pending_token != 0) {
				current_token = pending_token;
				val = pending_val;
				pending_token = 0;
				pending_val = null;
				return current_token;
			}

			int lastToken = current_token;
		restart:
			do
			{
				current_token = xtoken ();
				if (current_token == 0)
					return Token.EOF;
				if (current_token == Token.REM)
					current_token = DropComments();
			} while (lastToken == Token.EOL && current_token == Token.EOL);

			if (current_token == Token.HASH) {
				HandlePreprocessorDirective ();
				lastToken = Token.EOL;
				goto restart;
			}

			if (IsPreprocessorSkipping () && current_token != Token.EOF) {
				SkipInactiveLine ();
				if (current_token == Token.EOF || current_token == 0)
					return Token.EOF;

				lastToken = Token.EOL;
				goto restart;
			}

			// Merge "( Of" into OPEN_PARENS_OF only when the Of token
			// is on the same logical line. VB generic type-argument and
			// type-parameter lists use "(Of", but an end-of-line after
			// "(" should still terminate the statement unless some other
			// continuation rule applies.
			if (current_token == Token.OPEN_PARENS) {
				object saved_val = val;
				int next;
				next = xtoken ();
				if (next == 0)
					next = Token.EOF;
				else if (next == Token.REM)
					next = DropComments ();

				if (next == Token.OF) {
					current_token = Token.OPEN_PARENS_OF;
					val = saved_val;
				} else {
					pending_token = next;
					pending_val = val;
					val = saved_val;
					current_token = Token.OPEN_PARENS;
				}
			}

			if (current_token == Token.DOT &&
			    with_depth > 0 &&
			    !IsExpressionContinuation (lastToken)) {
				current_token = Token.DOT_WITH;
			}

			// Console.WriteLine ("Token = " + val);

			return current_token;
		}

		static bool IsExpressionContinuation (int tok)
		{
			switch (tok) {
			case Token.IDENTIFIER:
			case Token.CLOSE_PARENS:
			case Token.CLOSE_BRACKET:
			case Token.CLOSE_BRACE:
			case Token.LITERAL_INTEGER:
			case Token.LITERAL_DOUBLE:
			case Token.LITERAL_SINGLE:
			case Token.LITERAL_DECIMAL:
			case Token.LITERAL_STRING:
			case Token.LITERAL_CHARACTER:
			case Token.LITERAL_DATE:
			case Token.ME:
			case Token.MYBASE:
			case Token.MYCLASS:
			case Token.TRUE:
			case Token.FALSE:
			case Token.NOTHING:
				return true;
			default:
				return false;
			}
		}

		private string GetIdentifier()
		{
			int c = getChar();
			if (is_identifier_start_character ((char) c))
				return GetIdentifier(c);
			else
				return null;
		}

		private string GetIdentifier(int c)
		{
			StringBuilder id = new StringBuilder ();

			id.Append ((char) c);
				
			while ((c = peekChar ()) != -1) 
			{
				if (is_identifier_part_character ((char) c))
				{
					id.Append ((char)getChar ());
					col++;
				} 
				else 
					break;
			}
			
			cant_have_a_type_character = false;
			
			return id.ToString();
		}

		private bool is_doublequote(int currentChar)
		{
			return (currentChar == '"' || 
					currentChar == 0x201C || // unicode left double-quote character
					currentChar == 0x201D);  // unicode right double-quote character
		}
		
		private bool is_whitespace(int c)
		{
			// 0xFEFF is the BOM (U+FEFF); treat it like whitespace so
			// files that start with a BOM, or have one embedded by a
			// concatenating build step, do not trip the tokenizer.
			// 0xA0 is non-breaking space.
			return (c == ' ' || c == '\t' || c == '\v' || c == '\r' || c == 0xa0 || c == 0xfeff);
		}

		private void GobbleWhiteSpaces ()
		{
			int d = peekChar ();
			while (is_whitespace (d)) {
				getChar ();
				d = peekChar ();
			}

		}
		
		private bool tokens_seen = false;
		
		private void nextLine()
		{
			cant_have_a_type_character = true;
			line++;
			ref_line++;
			col = 0;
			tokens_seen = false;
		}

		public int xtoken ()
		{
			int t;
			bool doread = false;
			int c;

			val = null;
			for (;(c = getChar ()) != -1; col++) {
			
				// Handle line continuation character
				if (c == '_') 
				{
					// Conditional compilation skips inactive text after tokenization
					// decides which branch is active.  Inactive lines must not be
					// allowed to continue onto the next physical line first.
					if (IsPreprocessorSkipping ()) {
						error_details = "_";
						return Token.ERROR;
					}

					int d = peekChar();
					if (!is_identifier_part_character((char)d)) {
						while ((c = getChar ()) != -1 && !IsEOL(c)) {}
						c = getChar ();			
					}		
				}

				// white space
				if (is_whitespace(c)) {
					// expand tabs for location
					if (c == '\t')
						col = (((col + ExpandedTabsSize) / ExpandedTabsSize) * ExpandedTabsSize) - 1;
					cant_have_a_type_character = true;
					continue;
				}
				
				// Handle line comments.
				if (c == '\'')
					return Token.REM;					
				
				// Handle EOL.
				if (IsEOL(c))
				{
					if (current_token == Token.EOL) // if last token was also EOL keep skipping
						continue;
					return Token.EOL;
				}
				
				// Handle escaped identifiers
				if (c == '[')
				{
					if ((val = GetIdentifier()) == null)
						break;
					if ((c = getChar()) != ']')
						break;
					tokens_seen = true;
					return Token.IDENTIFIER;
				}

				// Handle unescaped identifiers
				if (is_identifier_start_character ((char) c))
				{
					string id;
					if ((id = GetIdentifier(c)) == null)
						break;
					val = id;
					tokens_seen = true;
					if (is_keyword(id) && (current_token != Token.DOT))
						return getKeyword(id);
					return Token.IDENTIFIER;
				}

				// Treat string literals
				if (is_doublequote(c)) {
					cant_have_a_type_character = true;
					return ExtractStringOrCharLiteral(c);
				}
			
				// handle numeric literals
				if (c == '.')
				{
					cant_have_a_type_character = true;
					tokens_seen = true;
					if (Char.IsDigit ((char) peekChar ()))
						return is_number (c);
					return Token.DOT;
				}
				
				if (Char.IsDigit ((char) c))
				{
					cant_have_a_type_character = true;
					tokens_seen = true;
					return is_number (c);
				}

				if ((t = is_punct ((char)c, ref doread)) != Token.ERROR) {
					cant_have_a_type_character = true;

					if (t == Token.NONE)
						continue;
						
					if (doread){
						getChar ();
						col++;
					}
					tokens_seen = true;
					return t;
				}
				
				error_details = ((char)c).ToString ();
				return Token.ERROR;
			}

			if (current_token != Token.EOL) // if last token wasn't EOL send it before EOF
				return Token.EOL;
			
			return Token.EOF;
		}

		private int ParseDateLiteral ()
		{
			int c, d;
			object temp;
			int month = 1, day = 1, year = 1, hours = 0, minutes = 0, seconds = 0, date_separator;
			bool minutes_specified = false, seconds_specified = false;
			bool am_specified = false, pm_specified = false;
			

			GobbleWhiteSpaces ();
			d = peekChar ();
			if (d == '#') 
				goto parse_error;

			temp = ParseIntLiteral ();
			if (temp == null)
				goto parse_error;

			d = peekChar ();
			if (d == '/' || d == '-') {
				c = getChar ();
				date_separator = c;

				month = (int) temp;
				// Console.WriteLine ("Month: " + month);

				temp = ParseIntLiteral ();
				if (temp == null)
					goto parse_error;
				day = (int) temp;
				// Console.WriteLine ("Day: " + day);

				c = getChar ();
				if (c != date_separator)
					goto parse_error;

				temp = ParseIntLiteral ();
				if (temp == null)
					goto parse_error;
				year = (int) temp;
				// Console.WriteLine ("Year: " + year);

				GobbleWhiteSpaces ();
				d = peekChar ();
				if (d == '#') {
					c = getChar ();
					goto parse_done;
				}

				temp = ParseIntLiteral ();
				if (temp == null) 
					goto parse_error;
				d = peekChar ();
			}

			hours = (int) temp;
			// Console.WriteLine ("Hours: " + hours);
			
			if (d == ':') {
				c = getChar ();
				
				temp = ParseIntLiteral ();
				if (temp == null)
					goto parse_error;
				minutes = (int) temp; 
				// Console.WriteLine ("Minutes: " + minutes);
				minutes_specified = true;

				d = peekChar ();
				if (d == ':') {
					c = getChar ();

					temp = ParseIntLiteral ();
					if (temp == null)
						goto parse_error;
					seconds = (int) temp;
					// Console.WriteLine ("Seconds: " + seconds);
					seconds_specified = true;
				} 
			}


			GobbleWhiteSpaces ();
			d = peekChar ();
			if (d == 'A' ) {
				c = getChar ();

				d = peekChar ();
				if (d != 'M')
					goto parse_error;

				c = getChar ();
				// Console.WriteLine ("AM");
				am_specified = true;
			} else if (d == 'P' ) {
				c = getChar ();

				d = peekChar ();
				if (d != 'M')
					goto parse_error;
				
				c = getChar ();
				// Console.WriteLine ("PM");
				pm_specified = true;
			}
			
			GobbleWhiteSpaces ();
			
			d = peekChar ();
			if (d == '#') {
				c = getChar ();
				if (!minutes_specified && !seconds_specified &&  !am_specified && ! pm_specified)
					goto parse_error;

				if ((am_specified || pm_specified) && hours > 12)
					goto parse_error;

				if (pm_specified)
					hours += 12;

				goto parse_done;
			}

		parse_error:
			// Console.WriteLine ("Parse Error");
			return Token.ERROR;

		parse_done:
			try {
				temp = new DateTime (year, month, day, hours, minutes, seconds);
			} catch (Exception ex) {
				// Console.WriteLine (ex);
				return Token.ERROR;
			}

			// Console.WriteLine ("Success");
			val =temp;
			return Token.LITERAL_DATE;
		}
		
		private int ExtractStringOrCharLiteral(int c)
		{
			StringBuilder s = new StringBuilder ();

			tokens_seen = true;

			while ((c = getChar ()) != -1){
				if (is_doublequote(c)){
					if (is_doublequote(peekChar()))
						getChar();
					else {
						//handle Char Literals
						if (peekChar() == 'C' || peekChar() == 'c') {
							getChar();
							if (s.Length == 1) {
								val = s[0];
								return Token.LITERAL_CHARACTER;
							} else {
								val = "Incorrect length for a character literal";
								return Token.ERROR;
							}							
						} else {
							val = s.ToString ();
							return Token.LITERAL_STRING;
						}
					}
				}

				if (IsEOL(c)) {
					return Token.ERROR;
				}
			
				s.Append ((char) c);
			}
					
			return Token.ERROR;
		}

		private object ParseIntLiteral ()
		{
			object retval;
			
			int d = peekChar ();
			
			if (!Char.IsDigit ((char) d))
				return null;

			int c = getChar ();
			number = new StringBuilder ();
			decimal_digits (c);

			try {
				retval = System.Int32.Parse (number.ToString ());
			} catch (Exception ex) {
				return null; 
			}

			return retval;
		}
 
		public void cleanup ()
		{
			if (pp_if_stack.Count > 0) {
				Report.Error (30012, Location, "#If must end with a matching #End If");
				pp_if_stack.Clear ();
			}

			if (pp_external_source) {
				Report.Error (30579, Location, "'#ExternalSource' directives must end with matching '#End ExternalSource'");
				pp_external_source = false;
			}

			if (pp_region_depth > 0) {
				Report.Error (30205, Location, "'#Region' directive must be followed by a matching '#End Region'");
				pp_region_depth = 0;
			}
		}

		public static void Cleanup () 
		{
		}

	}
}
