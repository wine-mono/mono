//
// cfold.cs: Constant Folding
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.
//

using System;

namespace Mono.CSharp {

	public class ConstantFold {

		//
		// Performs the numeric promotions on the left and right expresions
		// and desposits the results on `lc' and `rc'.
		//
		// On success, the types of `lc' and `rc' on output will always match,
		// and the pair will be one of:
		//
		//   (double, double)
		//   (float, float)
		//   (ulong, ulong)
		//   (long, long)
		//   (uint, uint)
		//   (int, int)
		//   (short, short)   (Happens with enumerations with underlying short type)
		//   (ushort, ushort) (Happens with enumerations with underlying short type)
		//
		static Constant ConvertConstantToType (Constant constant, Type target_type, Location loc)
		{
			if (constant.Type == target_type)
				return constant;

			if (target_type == TypeManager.double_type)
				return constant.ToDouble (loc);

			if (target_type == TypeManager.float_type)
				return constant.ToFloat (loc);

			if (target_type == TypeManager.decimal_type) {
				Constant integral = Convert.ConvertIntegralConstant (constant, target_type);
				if (integral != null)
					return integral;

				return constant.ToDecimal (loc);
			}

			if (TypeManager.IsFixedNumericType (target_type))
				return Convert.ConvertIntegralConstant (constant, target_type);

			return null;
		}

		static int ComparePromotedConstants (Constant left, Constant right)
		{
			return ((IComparable) left.GetValue ()).CompareTo (right.GetValue ());
		}

		static Constant FoldShiftConstant (Constant left, int shift, bool is_left_shift)
		{
			if (left is ByteConstant) {
				byte value = ((ByteConstant) left).Value;
				return new ByteConstant (is_left_shift ? (byte) (value << shift) : (byte) (value >> shift));
			}

			if (left is SByteConstant) {
				sbyte value = ((SByteConstant) left).Value;
				return new SByteConstant (is_left_shift ? (sbyte) (value << shift) : (sbyte) (value >> shift));
			}

			if (left is UShortConstant) {
				ushort value = ((UShortConstant) left).Value;
				return new UShortConstant (is_left_shift ? (ushort) (value << shift) : (ushort) (value >> shift));
			}

			if (left is ShortConstant) {
				short value = ((ShortConstant) left).Value;
				return new ShortConstant (is_left_shift ? (short) (value << shift) : (short) (value >> shift));
			}

			if (left is UIntConstant) {
				uint value = ((UIntConstant) left).Value;
				return new UIntConstant (is_left_shift ? value << shift : value >> shift);
			}

			if (left is IntConstant) {
				int value = ((IntConstant) left).Value;
				return new IntConstant (is_left_shift ? value << shift : value >> shift);
			}

			if (left is ULongConstant) {
				ulong value = ((ULongConstant) left).Value;
				return new ULongConstant (is_left_shift ? value << shift : value >> shift);
			}

			if (left is LongConstant) {
				long value = ((LongConstant) left).Value;
				return new LongConstant (is_left_shift ? value << shift : value >> shift);
			}

			return null;
		}

		static void DoConstantNumericPromotions (EmitContext ec, Binary.Operator oper,
							 ref Constant left, ref Constant right,
							 Location loc)
		{
			if (left is EnumConstant || right is EnumConstant){
				//
				// If either operand is an enum constant, the other one must
				// be implicitly convertable to that enum's underlying type.
				//
				EnumConstant match;
				Constant other;
				if (left is EnumConstant){
					other = right;
					match = (EnumConstant) left;
				} else {
					other = left;
					match = (EnumConstant) right;
				}

				bool need_check = (other is EnumConstant) ||
					((oper != Binary.Operator.Addition) &&
					 (oper != Binary.Operator.Subtraction));

				if (need_check &&
				    !Convert.WideningConversionExists (ec, match, other.Type)) {
					Convert.Error_CannotWideningConversion (loc, match.Type, other.Type);
					left = null;
					right = null;
					return;
				}

				if (left is EnumConstant)
					left = ((EnumConstant) left).Child;
				if (right is EnumConstant)
					right = ((EnumConstant) right).Child;

				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				return;
			}

			Type target_type = null;

			if (oper == Binary.Operator.Division) {
				if (left is DoubleConstant || right is DoubleConstant)
					target_type = TypeManager.double_type;
				else if (left is FloatConstant || right is FloatConstant)
					target_type = TypeManager.float_type;
				else if (left is DecimalConstant || right is DecimalConstant)
					target_type = TypeManager.decimal_type;
				else if (TypeManager.IsFixedNumericType (left.Type) &&
					 TypeManager.IsFixedNumericType (right.Type))
					target_type = TypeManager.double_type;
			} else {
				if (left is DoubleConstant || right is DoubleConstant)
					target_type = TypeManager.double_type;
				else if (left is FloatConstant || right is FloatConstant)
					target_type = TypeManager.float_type;
				else if (left is DecimalConstant || right is DecimalConstant)
					target_type = TypeManager.decimal_type;
				else
					target_type = TypeManager.GetVBBinaryIntegralResultType (oper, left.Type, right.Type);
			}

			if (target_type == null) {
				left = null;
				right = null;
				return;
			}

			left = ConvertConstantToType (left, target_type, loc);
			right = ConvertConstantToType (right, target_type, loc);
		}

		static void Error_CompileTimeOverflow (Location loc)
		{
			Report.Error (220, loc, "The operation overflows at compile time in checked mode");
		}
		
		/// <summary>
		///   Constant expression folder for binary operations.
		///
		///   Returns null if the expression can not be folded.
		/// </summary>
		static public Expression BinaryFold (EmitContext ec, Binary.Operator oper,
						     Constant left, Constant right, Location loc)
		{
			Type lt = left.Type;
			Type rt = right.Type;
			Type result_type = null;
			bool bool_res;

			//
			// Enumerator folding
			//
			if (rt == lt && left is EnumConstant)
				result_type = lt;

			//
			// During an enum evaluation, we need to unwrap enumerations
			//
			if (ec.InEnumContext){
				if (left is EnumConstant)
					left = ((EnumConstant) left).Child;
				
				if (right is EnumConstant)
					right = ((EnumConstant) right).Child;
			}

			Type wrap_as;
			Constant result = null;
			switch (oper){
			case Binary.Operator.BitwiseOr:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				if (left is IntConstant){
					IntConstant v;
					int res = ((IntConstant) left).Value | ((IntConstant) right).Value;
					
					v = new IntConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UIntConstant){
					UIntConstant v;
					uint res = ((UIntConstant)left).Value | ((UIntConstant)right).Value;
					
					v = new UIntConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is LongConstant){
					LongConstant v;
					long res = ((LongConstant)left).Value | ((LongConstant)right).Value;
					
					v = new LongConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ULongConstant){
					ULongConstant v;
					ulong res = ((ULongConstant)left).Value |
						((ULongConstant)right).Value;
					
					v = new ULongConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UShortConstant){
					UShortConstant v;
					ushort res = (ushort) (((UShortConstant)left).Value |
							       ((UShortConstant)right).Value);
					
					v = new UShortConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ShortConstant){
					ShortConstant v;
					short res = (short) (((ShortConstant)left).Value |
							     ((ShortConstant)right).Value);
					
					v = new ShortConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ByteConstant){
					ByteConstant v;
					byte res = (byte) (((ByteConstant)left).Value |
							   ((ByteConstant)right).Value);

					v = new ByteConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is SByteConstant){
					SByteConstant v;
					sbyte res = (sbyte) (((SByteConstant)left).Value |
							     ((SByteConstant)right).Value);

					v = new SByteConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				}
				break;
				
			case Binary.Operator.BitwiseAnd:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;
				
				if (left is IntConstant){
					IntConstant v;
					int res = ((IntConstant) left).Value & ((IntConstant) right).Value;
					
					v = new IntConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UIntConstant){
					UIntConstant v;
					uint res = ((UIntConstant)left).Value & ((UIntConstant)right).Value;
					
					v = new UIntConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is LongConstant){
					LongConstant v;
					long res = ((LongConstant)left).Value & ((LongConstant)right).Value;
					
					v = new LongConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ULongConstant){
					ULongConstant v;
					ulong res = ((ULongConstant)left).Value &
						((ULongConstant)right).Value;
					
					v = new ULongConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UShortConstant){
					UShortConstant v;
					ushort res = (ushort) (((UShortConstant)left).Value &
							       ((UShortConstant)right).Value);
					
					v = new UShortConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ShortConstant){
					ShortConstant v;
					short res = (short) (((ShortConstant)left).Value &
							     ((ShortConstant)right).Value);
					
					v = new ShortConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ByteConstant){
					ByteConstant v;
					byte res = (byte) (((ByteConstant)left).Value &
							   ((ByteConstant)right).Value);

					v = new ByteConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is SByteConstant){
					SByteConstant v;
					sbyte res = (sbyte) (((SByteConstant)left).Value &
							     ((SByteConstant)right).Value);

					v = new SByteConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				}
				break;

			case Binary.Operator.ExclusiveOr:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;
				
				if (left is IntConstant){
					IntConstant v;
					int res = ((IntConstant) left).Value ^ ((IntConstant) right).Value;
					
					v = new IntConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UIntConstant){
					UIntConstant v;
					uint res = ((UIntConstant)left).Value ^ ((UIntConstant)right).Value;
					
					v = new UIntConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is LongConstant){
					LongConstant v;
					long res = ((LongConstant)left).Value ^ ((LongConstant)right).Value;
					
					v = new LongConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ULongConstant){
					ULongConstant v;
					ulong res = ((ULongConstant)left).Value ^
						((ULongConstant)right).Value;
					
					v = new ULongConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UShortConstant){
					UShortConstant v;
					ushort res = (ushort) (((UShortConstant)left).Value ^
							       ((UShortConstant)right).Value);
					
					v = new UShortConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ShortConstant){
					ShortConstant v;
					short res = (short)(((ShortConstant)left).Value ^
							    ((ShortConstant)right).Value);
					
					v = new ShortConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ByteConstant){
					ByteConstant v;
					byte res = (byte) (((ByteConstant)left).Value ^
							   ((ByteConstant)right).Value);

					v = new ByteConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is SByteConstant){
					SByteConstant v;
					sbyte res = (sbyte) (((SByteConstant)left).Value ^
							     ((SByteConstant)right).Value);

					v = new SByteConstant (res);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				}
				break;

			case Binary.Operator.Addition:
				bool left_is_string = left is StringConstant;
				bool right_is_string = right is StringConstant;

				//
				// If both sides are strings, then concatenate, if
				// one is a string, and the other is not, then defer
				// to runtime concatenation
				//
				wrap_as = null;
				if (left_is_string || right_is_string){
					if (left_is_string && right_is_string)
						return new StringConstant (
							((StringConstant) left).Value +
							((StringConstant) right).Value);
					
					return null;
				}

				//
				// handle "E operator + (E x, U y)"
				// handle "E operator + (Y y, E x)"
				//
				// note that E operator + (E x, E y) is invalid
				//
				if (left is EnumConstant){
					if (right is EnumConstant){
						return null;
					}
					if (((EnumConstant) left).Child.Type != right.Type)
						return null;

					wrap_as = left.Type;
				} else if (right is EnumConstant){
					if (((EnumConstant) right).Child.Type != left.Type)
						return null;
					wrap_as = right.Type;
				}

				result = null;
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				try {
					if (left is DoubleConstant){
						double res;
						
						if (ec.ConstantCheckState)
							res = checked (((DoubleConstant) left).Value +
								       ((DoubleConstant) right).Value);
						else
							res = unchecked (((DoubleConstant) left).Value +
									 ((DoubleConstant) right).Value);
						
						result = new DoubleConstant (res);
					} else if (left is FloatConstant){
						float res;
						
						if (ec.ConstantCheckState)
							res = checked (((FloatConstant) left).Value +
								       ((FloatConstant) right).Value);
						else
							res = unchecked (((FloatConstant) left).Value +
									 ((FloatConstant) right).Value);
						
						result = new FloatConstant (res);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value +
								       ((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value +
									 ((ULongConstant) right).Value);

						result = new ULongConstant (res);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value +
								       ((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value +
									 ((LongConstant) right).Value);
						
						result = new LongConstant (res);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value +
								       ((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value +
									 ((UIntConstant) right).Value);
						
						result = new UIntConstant (res);
					} else if (left is UShortConstant){
						ushort res;

						if (ec.ConstantCheckState)
							res = checked ((ushort) (((UShortConstant) left).Value +
								       ((UShortConstant) right).Value));
						else
							res = unchecked ((ushort) (((UShortConstant) left).Value +
									 ((UShortConstant) right).Value));

						result = new UShortConstant (res);
					} else if (left is ShortConstant){
						short res;

						if (ec.ConstantCheckState)
							res = checked ((short) (((ShortConstant) left).Value +
								       ((ShortConstant) right).Value));
						else
							res = unchecked ((short) (((ShortConstant) left).Value +
									 ((ShortConstant) right).Value));

						result = new ShortConstant (res);
					} else if (left is ByteConstant){
						byte res;

						if (ec.ConstantCheckState)
							res = checked ((byte) (((ByteConstant) left).Value +
								       ((ByteConstant) right).Value));
						else
							res = unchecked ((byte) (((ByteConstant) left).Value +
									 ((ByteConstant) right).Value));

						result = new ByteConstant (res);
					} else if (left is SByteConstant){
						sbyte res;

						if (ec.ConstantCheckState)
							res = checked ((sbyte) (((SByteConstant) left).Value +
								       ((SByteConstant) right).Value));
						else
							res = unchecked ((sbyte) (((SByteConstant) left).Value +
									 ((SByteConstant) right).Value));

						result = new SByteConstant (res);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value +
								       ((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value +
									 ((IntConstant) right).Value);

						result = new IntConstant (res);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value +
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value +
								((DecimalConstant) right).Value);

						result = new DecimalConstant (res);
					} else {
						throw new Exception ( "Unexepected addition input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (loc);
				}

				if (wrap_as != null)
					return new EnumConstant (result, wrap_as);
				else
					return result;

			case Binary.Operator.Subtraction:
				//
				// handle "E operator - (E x, U y)"
				// handle "E operator - (Y y, E x)"
				// handle "U operator - (E x, E y)"
				//
				wrap_as = null;
				if (left is EnumConstant){
					if (right is EnumConstant){
						if (left.Type == right.Type)
							wrap_as = TypeManager.EnumToUnderlying (left.Type);
						else
							return null;
					}
					if (((EnumConstant) left).Child.Type != right.Type)
						return null;

					wrap_as = left.Type;
				} else if (right is EnumConstant){
					if (((EnumConstant) right).Child.Type != left.Type)
						return null;
					wrap_as = right.Type;
				}

				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				try {
					if (left is DoubleConstant){
						double res;
						
						if (ec.ConstantCheckState)
							res = checked (((DoubleConstant) left).Value -
								       ((DoubleConstant) right).Value);
						else
							res = unchecked (((DoubleConstant) left).Value -
									 ((DoubleConstant) right).Value);
						
						result = new DoubleConstant (res);
					} else if (left is FloatConstant){
						float res;
						
						if (ec.ConstantCheckState)
							res = checked (((FloatConstant) left).Value -
								       ((FloatConstant) right).Value);
						else
							res = unchecked (((FloatConstant) left).Value -
									 ((FloatConstant) right).Value);
						
						result = new FloatConstant (res);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value -
								       ((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value -
									 ((ULongConstant) right).Value);
						
						result = new ULongConstant (res);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value -
								       ((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value -
									 ((LongConstant) right).Value);
						
						result = new LongConstant (res);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value -
								       ((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value -
									 ((UIntConstant) right).Value);
						
						result = new UIntConstant (res);
					} else if (left is UShortConstant){
						ushort res;

						if (ec.ConstantCheckState)
							res = checked ((ushort) (((UShortConstant) left).Value -
								       ((UShortConstant) right).Value));
						else
							res = unchecked ((ushort) (((UShortConstant) left).Value -
									 ((UShortConstant) right).Value));

						result = new UShortConstant (res);
					} else if (left is ShortConstant){
						short res;

						if (ec.ConstantCheckState)
							res = checked ((short) (((ShortConstant) left).Value -
								       ((ShortConstant) right).Value));
						else
							res = unchecked ((short) (((ShortConstant) left).Value -
									 ((ShortConstant) right).Value));

						result = new ShortConstant (res);
					} else if (left is ByteConstant){
						byte res;

						if (ec.ConstantCheckState)
							res = checked ((byte) (((ByteConstant) left).Value -
								       ((ByteConstant) right).Value));
						else
							res = unchecked ((byte) (((ByteConstant) left).Value -
									 ((ByteConstant) right).Value));

						result = new ByteConstant (res);
					} else if (left is SByteConstant){
						sbyte res;

						if (ec.ConstantCheckState)
							res = checked ((sbyte) (((SByteConstant) left).Value -
								       ((SByteConstant) right).Value));
						else
							res = unchecked ((sbyte) (((SByteConstant) left).Value -
									 ((SByteConstant) right).Value));

						result = new SByteConstant (res);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value -
								       ((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value -
									 ((IntConstant) right).Value);

						result = new IntConstant (res);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value -
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value -
								((DecimalConstant) right).Value);

						return new DecimalConstant (res);
					} else {
						throw new Exception ( "Unexepected subtraction input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (loc);
				}
				if (wrap_as != null)
					return new EnumConstant (result, wrap_as);
				else
					return result;
				
			case Binary.Operator.Multiply:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				try {
					if (left is DoubleConstant){
						double res;
						
						if (ec.ConstantCheckState)
							res = checked (((DoubleConstant) left).Value *
								((DoubleConstant) right).Value);
						else
							res = unchecked (((DoubleConstant) left).Value *
								((DoubleConstant) right).Value);
						
						return new DoubleConstant (res);
					} else if (left is FloatConstant){
						float res;
						
						if (ec.ConstantCheckState)
							res = checked (((FloatConstant) left).Value *
								((FloatConstant) right).Value);
						else
							res = unchecked (((FloatConstant) left).Value *
								((FloatConstant) right).Value);
						
						return new FloatConstant (res);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value *
								((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value *
								((ULongConstant) right).Value);
						
						return new ULongConstant (res);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value *
								((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value *
								((LongConstant) right).Value);
						
						return new LongConstant (res);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value *
								((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value *
									 ((UIntConstant) right).Value);
						
						return new UIntConstant (res);
					} else if (left is UShortConstant){
						ushort res;

						if (ec.ConstantCheckState)
							res = checked ((ushort) (((UShortConstant) left).Value *
								       ((UShortConstant) right).Value));
						else
							res = unchecked ((ushort) (((UShortConstant) left).Value *
									 ((UShortConstant) right).Value));

						return new UShortConstant (res);
					} else if (left is ShortConstant){
						short res;

						if (ec.ConstantCheckState)
							res = checked ((short) (((ShortConstant) left).Value *
								       ((ShortConstant) right).Value));
						else
							res = unchecked ((short) (((ShortConstant) left).Value *
									 ((ShortConstant) right).Value));

						return new ShortConstant (res);
					} else if (left is ByteConstant){
						byte res;

						if (ec.ConstantCheckState)
							res = checked ((byte) (((ByteConstant) left).Value *
								       ((ByteConstant) right).Value));
						else
							res = unchecked ((byte) (((ByteConstant) left).Value *
									 ((ByteConstant) right).Value));

						return new ByteConstant (res);
					} else if (left is SByteConstant){
						sbyte res;

						if (ec.ConstantCheckState)
							res = checked ((sbyte) (((SByteConstant) left).Value *
								       ((SByteConstant) right).Value));
						else
							res = unchecked ((sbyte) (((SByteConstant) left).Value *
									 ((SByteConstant) right).Value));

						return new SByteConstant (res);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value *
								((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value *
								((IntConstant) right).Value);

						return new IntConstant (res);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value *
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value *
								((DecimalConstant) right).Value);

						return new DecimalConstant (res);
					} else {
						throw new Exception ( "Unexepected multiply input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (loc);
				}
				break;

			case Binary.Operator.Division:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				try {
					if (left is DoubleConstant){
						double res;
						
						if (ec.ConstantCheckState)
							res = checked (((DoubleConstant) left).Value /
								((DoubleConstant) right).Value);
						else
							res = unchecked (((DoubleConstant) left).Value /
								((DoubleConstant) right).Value);
						
						return new DoubleConstant (res);
					} else if (left is FloatConstant){
						float res;
						
						if (ec.ConstantCheckState)
							res = checked (((FloatConstant) left).Value /
								((FloatConstant) right).Value);
						else
							res = unchecked (((FloatConstant) left).Value /
								((FloatConstant) right).Value);
						
						return new FloatConstant (res);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value /
								((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value /
								((ULongConstant) right).Value);
						
						return new ULongConstant (res);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value /
								((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value /
								((LongConstant) right).Value);
						
						return new LongConstant (res);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value /
								((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value /
								((UIntConstant) right).Value);
						
						return new UIntConstant (res);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value /
								((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value /
								((IntConstant) right).Value);

						return new IntConstant (res);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value /
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value /
								((DecimalConstant) right).Value);

						return new DecimalConstant (res);
					} else {
						throw new Exception ( "Unexepected division input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (loc);

				} catch (DivideByZeroException) {
					Report.Error (020, loc, "Division by constant zero");
				}
				
				break;
				
			case Binary.Operator.Modulus:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				try {
					if (left is DoubleConstant){
						double res;
						
						if (ec.ConstantCheckState)
							res = checked (((DoubleConstant) left).Value %
								       ((DoubleConstant) right).Value);
						else
							res = unchecked (((DoubleConstant) left).Value %
									 ((DoubleConstant) right).Value);
						
						return new DoubleConstant (res);
					} else if (left is FloatConstant){
						float res;
						
						if (ec.ConstantCheckState)
							res = checked (((FloatConstant) left).Value %
								       ((FloatConstant) right).Value);
						else
							res = unchecked (((FloatConstant) left).Value %
									 ((FloatConstant) right).Value);
						
						return new FloatConstant (res);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value %
								       ((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value %
									 ((ULongConstant) right).Value);
						
						return new ULongConstant (res);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value %
								       ((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value %
									 ((LongConstant) right).Value);
						
						return new LongConstant (res);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value %
								       ((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value %
									 ((UIntConstant) right).Value);
						
						return new UIntConstant (res);
					} else if (left is UShortConstant){
						ushort res;

						if (ec.ConstantCheckState)
							res = checked ((ushort) (((UShortConstant) left).Value %
								       ((UShortConstant) right).Value));
						else
							res = unchecked ((ushort) (((UShortConstant) left).Value %
									 ((UShortConstant) right).Value));

						return new UShortConstant (res);
					} else if (left is ShortConstant){
						short res;

						if (ec.ConstantCheckState)
							res = checked ((short) (((ShortConstant) left).Value %
								       ((ShortConstant) right).Value));
						else
							res = unchecked ((short) (((ShortConstant) left).Value %
									 ((ShortConstant) right).Value));

						return new ShortConstant (res);
					} else if (left is ByteConstant){
						byte res;

						if (ec.ConstantCheckState)
							res = checked ((byte) (((ByteConstant) left).Value %
								       ((ByteConstant) right).Value));
						else
							res = unchecked ((byte) (((ByteConstant) left).Value %
									 ((ByteConstant) right).Value));

						return new ByteConstant (res);
					} else if (left is SByteConstant){
						sbyte res;

						if (ec.ConstantCheckState)
							res = checked ((sbyte) (((SByteConstant) left).Value %
								       ((SByteConstant) right).Value));
						else
							res = unchecked ((sbyte) (((SByteConstant) left).Value %
									 ((SByteConstant) right).Value));

						return new SByteConstant (res);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value %
								       ((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value %
									 ((IntConstant) right).Value);

						return new IntConstant (res);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value %
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value %
								((DecimalConstant) right).Value);

						return new DecimalConstant (res);
					} else {
						throw new Exception ( "Unexepected modulus input: " + left);
					}
				} catch (DivideByZeroException){
					Report.Error (020, loc, "Division by constant zero");
				} catch (OverflowException){
					Error_CompileTimeOverflow (loc);
				}
				break;

				//
				// There is no overflow checking on left shift
				//
			case Binary.Operator.LeftShift:
				IntConstant ic = right.ToInt (loc);
				if (ic == null){
					Binary.Error_OperatorCannotBeApplied (loc, "<<", lt, rt);
					return null;
				}
				int lshift_val = ic.Value;

				result = FoldShiftConstant (left, lshift_val, true);
				if (result != null)
					return result;

				Binary.Error_OperatorCannotBeApplied (loc, "<<", lt, rt);
				break;

				//
				// There is no overflow checking on right shift
				//
			case Binary.Operator.RightShift:
				IntConstant sic = right.ToInt (loc);
				if (sic == null){
					Binary.Error_OperatorCannotBeApplied (loc, ">>", lt, rt);
					return null;
				}
				int rshift_val = sic.Value;

				result = FoldShiftConstant (left, rshift_val, false);
				if (result != null)
					return result;

				Binary.Error_OperatorCannotBeApplied (loc, ">>", lt, rt);
				break;

			case Binary.Operator.LogicalAndAlso:
				if (left is BoolConstant && right is BoolConstant){
					return new BoolConstant (
						((BoolConstant) left).Value &&
						((BoolConstant) right).Value);
				}
				break;

			case Binary.Operator.LogicalOrElse:
				if (left is BoolConstant && right is BoolConstant){
					return new BoolConstant (
						((BoolConstant) left).Value ||
						((BoolConstant) right).Value);
				}
				break;
				
			case Binary.Operator.Equality:
				if (left is BoolConstant && right is BoolConstant){
					return new BoolConstant (
						((BoolConstant) left).Value ==
						((BoolConstant) right).Value);
				
				}
				if (left is NullLiteral){
					if (right is NullLiteral)
						return new BoolConstant (true);
					else if (right is StringConstant)
						return new BoolConstant (
							((StringConstant) right).Value == null);
				} else if (right is NullLiteral){
					if (left is NullLiteral)
						return new BoolConstant (true);
					else if (left is StringConstant)
						return new BoolConstant (
							((StringConstant) left).Value == null);
				}
				if (left is StringConstant && right is StringConstant){
					return new BoolConstant (
						((StringConstant) left).Value ==
						((StringConstant) right).Value);
					
				}

				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				return new BoolConstant (ComparePromotedConstants (left, right) == 0);

			case Binary.Operator.Inequality:
				if (left is BoolConstant && right is BoolConstant){
					return new BoolConstant (
						((BoolConstant) left).Value !=
						((BoolConstant) right).Value);
				}
				if (left is NullLiteral){
					if (right is NullLiteral)
						return new BoolConstant (false);
					else if (right is StringConstant)
						return new BoolConstant (
							((StringConstant) right).Value != null);
				} else if (right is NullLiteral){
					if (left is NullLiteral)
						return new BoolConstant (false);
					else if (left is StringConstant)
						return new BoolConstant (
							((StringConstant) left).Value != null);
				}
				if (left is StringConstant && right is StringConstant){
					return new BoolConstant (
						((StringConstant) left).Value !=
						((StringConstant) right).Value);
					
				}
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				return new BoolConstant (ComparePromotedConstants (left, right) != 0);

			case Binary.Operator.LessThan:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				return new BoolConstant (ComparePromotedConstants (left, right) < 0);
				
			case Binary.Operator.GreaterThan:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				return new BoolConstant (ComparePromotedConstants (left, right) > 0);

			case Binary.Operator.GreaterThanOrEqual:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				return new BoolConstant (ComparePromotedConstants (left, right) >= 0);

			case Binary.Operator.LessThanOrEqual:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				return new BoolConstant (ComparePromotedConstants (left, right) <= 0);
			}
					
			return null;
		}
	}
}
