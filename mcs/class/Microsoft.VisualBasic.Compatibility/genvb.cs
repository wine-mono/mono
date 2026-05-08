// 
// genvb.cs
//
// Author:
//   Esme Povirk <esme@codeweavers.com>
//
// Copyright (C) 2023 CodeWeavers, Inc
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// Preprocesses VB.NET code, generating the various Array types in
// Microsoft.VisualBasic.Compatibility.

using System;
using System.Collections.Generic;
using System.IO;

public class GenVB
{
	static string ReplaceDefines (string input, Dictionary<string, string> defines)
	{
		foreach (var kvp in defines)
		{
			input = input.Replace (kvp.Key, kvp.Value);
		}

		return input;
	}

	static int SkipIfBody (TextReader input, string inputFileName)
	{
		int ifDepth = 1;
		int linesSkipped = 0;
		string line;

		while (ifDepth > 0 && (line = input.ReadLine ()) != null)
		{
			if (line.StartsWith("#if"))
				ifDepth++;
			else if (line.StartsWith("#endif"))
				ifDepth--;
			linesSkipped++;
		}

		if (ifDepth != 0)
			throw new Exception(string.Format ("#ifdef or #ifndef without matching #endif in {0}", inputFileName));

		return linesSkipped;
	}

	static void ProcessFile (string inputFileName, TextWriter output, Dictionary<string, string> defines)
	{
		var input = new StreamReader (inputFileName);
		string line;
		int lineNum = 0;
		int ifDepth = 0;

		output.WriteLine (string.Format("'BEGIN contents of {0}", inputFileName));

		while ((line = input.ReadLine ()) != null)
		{
			lineNum++;

			if (line.StartsWith ("#define"))
			{
				var substrings = line.Split(default(char[]), 3, StringSplitOptions.RemoveEmptyEntries);
				if (substrings[0] != "#define")
					throw new Exception(string.Format ("Unrecognized directive at {0}:{1}", inputFileName, lineNum));
				if (substrings.Length < 2)
					throw new Exception(string.Format ("Expected identifier after #define at {0}:{1}", inputFileName, lineNum));

				string val;
				if (substrings.Length == 3)
					val = ReplaceDefines (substrings[2], defines);
				else
					val = string.Empty;

				defines [substrings[1]] = val;
			}
			else if (line.StartsWith ("#ifdef"))
			{
				var substrings = line.Split(default(char[]), 2, StringSplitOptions.RemoveEmptyEntries);

				if (substrings[0] != "#ifdef")
					throw new Exception(string.Format ("Unrecognized directive at {0}:{1}", inputFileName, lineNum));
				if (substrings.Length < 2)
					throw new Exception(string.Format ("Expected identifier after #ifdef at {0}:{1}", inputFileName, lineNum));

				if (defines.ContainsKey (substrings[1]))
					ifDepth++;
				else
					lineNum += SkipIfBody (input, inputFileName);
			}
			else if (line.StartsWith ("#ifndef"))
			{
				var substrings = line.Split(default(char[]), 2, StringSplitOptions.RemoveEmptyEntries);

				if (substrings[0] != "#ifndef")
					throw new Exception(string.Format ("Unrecognized directive at {0}:{1}", inputFileName, lineNum));
				if (substrings.Length < 2)
					throw new Exception(string.Format ("Expected identifier after #ifndef at {0}:{1}", inputFileName, lineNum));

				if (!defines.ContainsKey (substrings[1]))
					ifDepth++;
				else
					lineNum += SkipIfBody (input, inputFileName);
			}
			else if (line.StartsWith ("#endif"))
			{
				var substrings = line.Split(default(char[]), 2, StringSplitOptions.RemoveEmptyEntries);

				if (substrings[0] != "#endif")
					throw new Exception(string.Format ("Unrecognized directive at {0}:{1}", inputFileName, lineNum));
				if (substrings.Length != 1)
					throw new Exception(string.Format ("Unexpected argument to #endif at {0}:{1}", inputFileName, lineNum));
				if (lineNum == 0)
					throw new Exception(string.Format ("#endif without matching #ifdef or #ifndef at {0}:{1}", inputFileName, lineNum));

				ifDepth--;
			}
			else if (line.StartsWith ("#include"))
			{
				var substrings = line.Split(default(char[]), 2, StringSplitOptions.RemoveEmptyEntries);

				if (substrings[0] != "#include")
					throw new Exception(string.Format ("Unrecognized directive at {0}:{1}", inputFileName, lineNum));
				if (substrings.Length != 2)
					throw new Exception(string.Format ("Expected filename after #include at {0}:{1}", inputFileName, lineNum));

				ProcessFile (Path.Join (Path.GetDirectoryName (inputFileName), substrings[1].Trim('"')), output, defines);
			}
			else if (line.StartsWith ("#"))
			{
				throw new Exception(string.Format ("Unrecognized directive at {0}:{1}", inputFileName, lineNum));
			}
			else
			{
				output.WriteLine (ReplaceDefines (line, defines));
			}
		}

		if (ifDepth != 0)
		{
			throw new Exception(string.Format ("#ifdef or #ifndef without matching #endif in {0}", inputFileName));
		}

		output.WriteLine (string.Format("'END contents of {0}", inputFileName));
	}

	static int Main (string[] args)
	{
		if (args.Length < 2 || args[0] == "--help")
		{
			Console.WriteLine ("Usage: genvb InputFileName OutputFileName");
			return 1;
		}

		var inputFileName = args[0];
		var outputFileName = args[1];

		using (var output = new StreamWriter (outputFileName))
		{
			ProcessFile (inputFileName, output, new Dictionary<string, string> ());
		}

		return 0;
	}
}

