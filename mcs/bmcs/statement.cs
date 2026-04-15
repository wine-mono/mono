//
// statement.cs: Statement representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Martin Baulig (martin@ximian.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc.
// (C) 2003, 2004 Novell, Inc.
//

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Mono.CSharp {

	using System.Collections;

	// VB loop-control statements name the containing block kind.
	// `For' covers both `For ... Next' and `For Each ... Next'.
	public enum VBLoopKind {
		Do,
		For,
		While
	}
	
	public abstract class Statement {
		public Location loc;
		
		/// <summary>
		/// Resolves the statement, true means that all sub-statements
		/// did resolve ok.
		//  </summary>
		public virtual bool Resolve (EmitContext ec)
		{
			return true;
		}
		
		/// <summary>
		///   We already know that the statement is unreachable, but we still
		///   need to resolve it to catch errors.
		/// </summary>
		public virtual bool ResolveUnreachable (EmitContext ec, bool warn)
		{
			//
			// This conflicts with csc's way of doing this, but IMHO it's
			// the right thing to do.
			//
			// If something is unreachable, we still check whether it's
			// correct.  This means that you cannot use unassigned variables
			// in unreachable code, for instance.
			//

			if (warn && (RootContext.WarningLevel >= 2))
				Report.Warning (162, loc, "Unreachable code detected");

			ec.StartFlowBranching (FlowBranching.BranchingType.Block, loc);
			bool ok = Resolve (ec);
			ec.KillFlowBranching ();

			return ok;
		}
		
		protected void CheckObsolete (Type type)
		{
			ObsoleteAttribute obsolete_attr = AttributeTester.GetObsoleteAttribute (type);
			if (obsolete_attr == null)
				return;

			AttributeTester.Report_ObsoleteMessage (obsolete_attr, type.FullName, loc);
		}
		
		/// <summary>
		///   Return value indicates whether all code paths emitted return.
		/// </summary>
		protected abstract void DoEmit (EmitContext ec);

		/// <summary>
		///   Utility wrapper routine for Error, just to beautify the code
		/// </summary>
		public void Error (int error, string format, params object[] args)
		{
			Error (error, String.Format (format, args));
		}
		
		public void Error (int error, string s)
		{
			if (!Location.IsNull (loc))
				Report.Error (error, loc, s);
				else
				Report.Error (error, s);
		}

		/// <summary>
		///   Return value indicates whether all code paths emitted return.
		/// </summary>
		public virtual void Emit (EmitContext ec)
		{
			ec.Mark (loc, true);
			DoEmit (ec);
		}
	}

	public sealed class EmptyStatement : Statement {
		
		private EmptyStatement () {}
		
		public static readonly EmptyStatement Value = new EmptyStatement ();
		
		public override bool Resolve (EmitContext ec)
		{
			return true;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
		}
	}
	
	public class If : Statement {
		Expression expr;
		public Statement TrueStatement;
		public Statement FalseStatement;
		
		bool is_true_ret;
		
		public If (Expression expr, Statement trueStatement, Location l)
		{
			this.expr = expr;
			TrueStatement = trueStatement;
			loc = l;
		}

		public If (Expression expr,
			   Statement trueStatement,
			   Statement falseStatement,
			   Location l)
		{
			this.expr = expr;
			TrueStatement = trueStatement;
			FalseStatement = falseStatement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			Report.Debug (1, "START IF BLOCK", loc);

			expr = Expression.ResolveBoolean (ec, expr, loc);
			if (expr == null){
				return false;
			}
			
			Assign ass = expr as Assign;
			if (ass != null && ass.Source is Constant) {
				Report.Warning (665, 3, loc, "Assignment in conditional expression is always constant; did you mean to use == instead of = ?");
			}

			//
			// Dead code elimination
			//
			if (expr is BoolConstant){
				bool take = ((BoolConstant) expr).Value;

				if (take){
					if (!TrueStatement.Resolve (ec))
						return false;

					if ((FalseStatement != null) &&
					    !FalseStatement.ResolveUnreachable (ec, true))
						return false;
					FalseStatement = null;
				} else {
					if (!TrueStatement.ResolveUnreachable (ec, true))
						return false;
					TrueStatement = null;
			
					if ((FalseStatement != null) &&
					    !FalseStatement.Resolve (ec))
				return false;
			}

				return true;
			}
			
			ec.StartFlowBranching (FlowBranching.BranchingType.Conditional, loc);
			
			bool ok = TrueStatement.Resolve (ec);

			is_true_ret = ec.CurrentBranching.CurrentUsageVector.Reachability.IsUnreachable;

			ec.CurrentBranching.CreateSibling ();

			if ((FalseStatement != null) && !FalseStatement.Resolve (ec))
				ok = false;
					
			ec.EndFlowBranching ();

			Report.Debug (1, "END IF BLOCK", loc);

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label false_target = ig.DefineLabel ();
			Label end;

			//
			// If we're a boolean expression, Resolve() already
			// eliminated dead code for us.
			//
			if (expr is BoolConstant){
				bool take = ((BoolConstant) expr).Value;

				if (take)
					TrueStatement.Emit (ec);
				else if (FalseStatement != null)
						FalseStatement.Emit (ec);

						return;
					}
			
			expr.EmitBranchable (ec, false_target, false);

			TrueStatement.Emit (ec);

			if (FalseStatement != null){
				bool branch_emitted = false;
				
				end = ig.DefineLabel ();
				if (!is_true_ret){
					ig.Emit (OpCodes.Br, end);
					branch_emitted = true;
				}

				ig.MarkLabel (false_target);
				FalseStatement.Emit (ec);

				if (branch_emitted)
					ig.MarkLabel (end);
			} else {
				ig.MarkLabel (false_target);
			}
		}
	}

	public class Do : Statement {
		public Expression expr;
		public readonly Statement  EmbeddedStatement;
		readonly bool test_before;
		readonly bool until_condition;
		bool infinite, empty;
		
		public Do (Statement statement, Expression boolExpr, bool testBefore, bool untilCondition, Location l)
		{
			expr = boolExpr;
			EmbeddedStatement = statement;
			test_before = testBefore;
			until_condition = untilCondition;
			loc = l;
		}

		bool ContinueOnCondition (bool condition_value)
		{
			return until_condition ? !condition_value : condition_value;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			expr = Expression.ResolveBoolean (ec, expr, loc);
			if (expr == null)
				return false;

			if (expr is BoolConstant) {
				bool res = ContinueOnCondition (((BoolConstant) expr).Value);

				if (test_before && !res) {
					if (!EmbeddedStatement.ResolveUnreachable (ec, true))
						return false;
					empty = true;
					return true;
				}

				if (res)
					infinite = true;
			}

			ec.StartFlowBranching (VBLoopKind.Do, loc);

			if (!EmbeddedStatement.Resolve (ec))
				ok = false;

			ec.CurrentBranching.Infinite = infinite;
			ec.EndFlowBranching ();

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			if (empty)
				return;

			ILGenerator ig = ec.ig;
			Label loop = ig.DefineLabel ();
			Label test = ig.DefineLabel ();
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			
			ec.LoopBegin = test;
			ec.LoopEnd = ig.DefineLabel ();
			ec.PushVBLoop (VBLoopKind.Do, ec.LoopBegin, ec.LoopEnd);

			if (test_before)
				ig.Emit (OpCodes.Br, test);

			ig.MarkLabel (loop);
			EmbeddedStatement.Emit (ec);
			ig.MarkLabel (test);

			if (expr is BoolConstant) {
				bool res = ContinueOnCondition (((BoolConstant) expr).Value);
				if (res)
					ig.Emit (OpCodes.Br, loop);
			} else
				expr.EmitBranchable (ec, loop, !until_condition);
			
			ig.MarkLabel (ec.LoopEnd);
			ec.PopVBLoop ();

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}
	}

	public class While : Statement {
		public Expression expr;
		public readonly Statement Statement;
		bool infinite, empty;
		
		public While (Expression boolExpr, Statement statement, Location l)
		{
			this.expr = boolExpr;
			Statement = statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			expr = Expression.ResolveBoolean (ec, expr, loc);
			if (expr == null)
				return false;

			//
			// Inform whether we are infinite or not
			//
			if (expr is BoolConstant){
				BoolConstant bc = (BoolConstant) expr;

				if (bc.Value == false){
					if (!Statement.ResolveUnreachable (ec, true))
						return false;
					empty = true;
					return true;
				} else
					infinite = true;
			}

			ec.StartFlowBranching (VBLoopKind.While, loc);

			if (!Statement.Resolve (ec))
				ok = false;

				ec.CurrentBranching.Infinite = infinite;
				ec.EndFlowBranching ();

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			if (empty)
				return;

			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.PushVBLoop (VBLoopKind.While, ec.LoopBegin, ec.LoopEnd);

			//
			// Inform whether we are infinite or not
			//
			if (expr is BoolConstant){
				ig.MarkLabel (ec.LoopBegin);
				Statement.Emit (ec);
				ig.Emit (OpCodes.Br, ec.LoopBegin);
					
				//
				// Inform that we are infinite (ie, `we return'), only
				// if we do not `break' inside the code.
				//
				ig.MarkLabel (ec.LoopEnd);
			} else {
				Label while_loop = ig.DefineLabel ();

				ig.Emit (OpCodes.Br, ec.LoopBegin);
				ig.MarkLabel (while_loop);

				Statement.Emit (ec);
			
				ig.MarkLabel (ec.LoopBegin);

				expr.EmitBranchable (ec, while_loop, true);
				
				ig.MarkLabel (ec.LoopEnd);
			}	

			ec.PopVBLoop ();
			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}
	}

	public class For : Statement {
		Expression Test;
		readonly Statement InitStatement;
		readonly Statement Increment;
		readonly Statement Statement;
		bool infinite, empty;
		
		public For (Statement initStatement,
			    Expression test,
			    Statement increment,
			    Statement statement,
			    Location l)
		{
			InitStatement = initStatement;
			Test = test;
			Increment = increment;
			Statement = statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			if (InitStatement != null){
				if (!InitStatement.Resolve (ec))
					ok = false;
			}

			if (Test != null){
				Test = Expression.ResolveBoolean (ec, Test, loc);
				if (Test == null)
					ok = false;
				else if (Test is BoolConstant){
					BoolConstant bc = (BoolConstant) Test;

					if (bc.Value == false){
						if (!Statement.ResolveUnreachable (ec, true))
							return false;
						if ((Increment != null) &&
						    !Increment.ResolveUnreachable (ec, false))
							return false;
						empty = true;
						return true;
					} else
						infinite = true;
				}
			} else
				infinite = true;

			ec.StartFlowBranching (VBLoopKind.For, loc);
			if (!infinite)
				ec.CurrentBranching.CreateSibling ();

			if (!Statement.Resolve (ec))
				ok = false;

			if (Increment != null){
				if (!Increment.Resolve (ec))
					ok = false;
			}

				ec.CurrentBranching.Infinite = infinite;
				ec.EndFlowBranching ();

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			if (empty)
				return;

			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			Label loop = ig.DefineLabel ();
			Label test = ig.DefineLabel ();
			
			if (InitStatement != null && InitStatement != EmptyStatement.Value)
					InitStatement.Emit (ec);

			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.PushVBLoop (VBLoopKind.For, ec.LoopBegin, ec.LoopEnd);

			ig.Emit (OpCodes.Br, test);
			ig.MarkLabel (loop);
			Statement.Emit (ec);

			ig.MarkLabel (ec.LoopBegin);
			if (Increment != EmptyStatement.Value)
				Increment.Emit (ec);

			ig.MarkLabel (test);
			//
			// If test is null, there is no test, and we are just
			// an infinite loop
			//
			if (Test != null){
				//
				// The Resolve code already catches the case for
				// Test == BoolConstant (false) so we know that
				// this is true
				//
				if (Test is BoolConstant)
					ig.Emit (OpCodes.Br, loop);
				else
					Test.EmitBranchable (ec, loop, true);
				
			} else
				ig.Emit (OpCodes.Br, loop);
			ig.MarkLabel (ec.LoopEnd);
			ec.PopVBLoop ();

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}
	}

	// VB `For ... To ... [Step ...]' evaluates the initial value,
	// limit, and step exactly once in textual order when the loop
	// begins, and only then assigns the initial value to the loop
	// control variable.  Reusing bmcs's C-like For node directly would
	// re-evaluate the limit and step expressions every iteration, so
	// cache the three expressions into temporaries and keep the
	// loop-control test in one statement node.
	public class VBFor : Statement {
		readonly string identifier;
		Expression start;
		Expression limit;
		Expression step;
		readonly Statement statement;

		Statement init_statement;
		Statement increment_statement;
		Expression test;
		LocalTemporary start_temp;
		LocalTemporary limit_temp;
		LocalTemporary step_temp;

		public VBFor (string identifier, Expression start, Expression limit,
			Expression step, Statement statement, Location l)
		{
			this.identifier = identifier;
			this.start = start;
			this.limit = limit;
			this.step = step;
			this.statement = statement;
			loc = l;
		}

		Expression ResolveCachedBound (EmitContext ec, Expression expr, Type loop_type)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return null;

			return Convert.ImplicitVBConversionRequired (ec, expr, loop_type, loc);
		}

		Expression BuildTest ()
		{
			Expression counter_for_pos = new SimpleName (identifier, loc);
			Expression counter_for_neg = new SimpleName (identifier, loc);
			Expression zero = new IntLiteral (0);

			Expression pos_arm = new Binary (
				Binary.Operator.LogicalAndAlso,
				new Binary (Binary.Operator.GreaterThanOrEqual,
					step_temp, zero, loc),
				new Binary (Binary.Operator.LessThanOrEqual,
					counter_for_pos, limit_temp, loc),
				loc);
			Expression neg_arm = new Binary (
				Binary.Operator.LogicalAndAlso,
				new Binary (Binary.Operator.LessThan,
					step_temp, zero, loc),
				new Binary (Binary.Operator.GreaterThanOrEqual,
					counter_for_neg, limit_temp, loc),
				loc);

			return new Binary (Binary.Operator.LogicalOrElse, pos_arm, neg_arm, loc);
		}

		Statement BuildIncrement ()
		{
			CompoundAssign increment = new CompoundAssign (
				Binary.Operator.Addition,
				new SimpleName (identifier, loc), step_temp, loc);
			return new StatementExpression (increment, loc);
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;
			Expression loop_control = new SimpleName (identifier, loc).ResolveLValue (
				ec, EmptyExpression.Null);
			Type loop_type = loop_control != null ? loop_control.Type : null;
			if (loop_type == null)
				return false;

			start = ResolveCachedBound (ec, start, loop_type);
			if (start == null)
				ok = false;

			limit = ResolveCachedBound (ec, limit, loop_type);
			if (limit == null)
				ok = false;

			step = ResolveCachedBound (ec, step, loop_type);
			if (step == null)
				ok = false;

			if (!ok)
				return false;

			start_temp = new LocalTemporary (ec, loop_type);
			limit_temp = new LocalTemporary (ec, loop_type);
			step_temp = new LocalTemporary (ec, loop_type);

			init_statement = new StatementExpression (
				new Assign (new SimpleName (identifier, loc), start_temp, loc), loc);
			test = Expression.ResolveBoolean (ec, BuildTest (), loc);
			if (test == null)
				return false;

			increment_statement = BuildIncrement ();

			if (!init_statement.Resolve (ec))
				return false;

			ec.StartFlowBranching (VBLoopKind.For, loc);
			ec.CurrentBranching.CreateSibling ();

			if (!statement.Resolve (ec))
				ok = false;

			if (increment_statement != null && !increment_statement.Resolve (ec))
				ok = false;

			ec.EndFlowBranching ();

			return ok;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			Label loop = ig.DefineLabel ();
			Label test_label = ig.DefineLabel ();

			start.Emit (ec);
			start_temp.Store (ec);

			limit.Emit (ec);
			limit_temp.Store (ec);

			step.Emit (ec);
			step_temp.Store (ec);

			if (init_statement != null && init_statement != EmptyStatement.Value)
				init_statement.Emit (ec);

			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.PushVBLoop (VBLoopKind.For, ec.LoopBegin, ec.LoopEnd);

			ig.Emit (OpCodes.Br, test_label);
			ig.MarkLabel (loop);
			statement.Emit (ec);

			ig.MarkLabel (ec.LoopBegin);
			if (increment_statement != null && increment_statement != EmptyStatement.Value)
				increment_statement.Emit (ec);

			ig.MarkLabel (test_label);
			test.EmitBranchable (ec, loop, true);

			ig.MarkLabel (ec.LoopEnd);
			ec.PopVBLoop ();

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;

			start_temp.Release (ec);
			limit_temp.Release (ec);
			step_temp.Release (ec);
		}
	}
	
	public class StatementExpression : Statement {
		public ExpressionStatement expr;
		
		public StatementExpression (ExpressionStatement expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			expr = expr.ResolveStatement (ec);
			return expr != null;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			expr.EmitStatement (ec);
		}

		public override string ToString ()
		{
			return "StatementExpression (" + expr + ")";
		}
	}

	public class EventSubscription : Statement {
		Expression event_expr;
		Expression handler_expr;
		readonly bool is_add;
		EventExpr resolved_event;

		public EventSubscription (Expression eventExpr, Expression handlerExpr, bool isAdd, Location l)
		{
			event_expr = eventExpr;
			handler_expr = handlerExpr;
			is_add = isAdd;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			// AddHandler/RemoveHandler operate on event access directly; preserving
			// EventExpr here avoids the normal member-access rewrite that turns
			// field-like events into backing-field access in expression contexts.
			event_expr = event_expr.ResolveForEventAccess (ec);
			if (event_expr == null)
				return false;

			resolved_event = event_expr as EventExpr;
			if (resolved_event == null) {
				Error (33001, "'{0}' requires an event access as its first operand",
					is_add ? "AddHandler" : "RemoveHandler");
				return false;
			}

			handler_expr = handler_expr.Resolve (ec);
			if (handler_expr == null)
				return false;

			handler_expr = Convert.ImplicitVBConversionRequired (ec, handler_expr,
				resolved_event.Type, loc);
			return handler_expr != null;
		}

		protected override void DoEmit (EmitContext ec)
		{
			resolved_event.EmitAddOrRemove (ec, handler_expr, is_add);
		}
	}

	/// <summary>
	///   Implements the return statement
	/// </summary>
	public class Return : Statement {
		public Expression Expr;
		
		public Return (Expression expr, Location l)
		{
			Expr = expr;
			loc = l;
		}

		bool in_exc;

		public override bool Resolve (EmitContext ec)
		{
			if (ec.ReturnType == null){
			if (Expr != null){
					if (ec.CurrentAnonymousMethod != null){
						Report.Error (1662, loc, String.Format (
							"Anonymous method could not be converted to delegate " +
							"since the return value does not match the delegate value"));
					}
					Error (127, "Return with a value not allowed here");
					return false;
				}
			} else {
				if (Expr == null){
					Error (126, "An object of type `{0}' is expected " +
					       "for the return statement",
					       TypeManager.CSharpName (ec.ReturnType));
					return false;
				}

				Expr = Expr.Resolve (ec);
				if (Expr == null)
					return false;

				if (Expr.Type != ec.ReturnType) {
					Expr = Convert.WideningConversionRequired (
						ec, Expr, ec.ReturnType, loc);
					if (Expr == null)
						return false;
				}
			}

			if (ec.InIterator){
				Error (-206, "Return statement not allowed inside iterators");
				return false;
			}
				
			FlowBranching.UsageVector vector = ec.CurrentBranching.CurrentUsageVector;

			if (ec.CurrentBranching.InTryOrCatch (true)) {
				ec.CurrentBranching.AddFinallyVector (vector);
				in_exc = true;
			} else if (ec.CurrentBranching.InFinally (true)) {
				Error (157, "Control can not leave the body of the finally block");
				return false;
			} else
				vector.CheckOutParameters (ec.CurrentBranching);

			if (in_exc)
				ec.NeedReturnLabel ();

			ec.CurrentBranching.CurrentUsageVector.Return ();
			return true;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			if (Expr != null) {
				Expr.Emit (ec);

				if (in_exc)
					ec.ig.Emit (OpCodes.Stloc, ec.TemporaryReturn ());
			}

			if (in_exc)
				ec.ig.Emit (OpCodes.Leave, ec.ReturnLabel);
			else
				ec.ig.Emit (OpCodes.Ret);
			}
		}

	public class Goto : Statement {
		string target;
		Block block;
		LabeledStatement label;
		
		public override bool Resolve (EmitContext ec)
		{
			label = ec.CurrentBranching.LookupLabel (target, loc);
			if (label == null)
				return false;

			// If this is a forward goto.
			if (!label.IsDefined)
				label.AddUsageVector (ec.CurrentBranching.CurrentUsageVector);

			ec.CurrentBranching.CurrentUsageVector.Goto ();
			label.AddReference ();

			return true;
		}
		
		public Goto (Block parent_block, string label, Location l)
		{
			block = parent_block;
			loc = l;
			target = label;
		}

		public string Target {
			get {
				return target;
			}
		}

		protected override void DoEmit (EmitContext ec)
		{
			Label l = label.LabelTarget (ec);
			ec.ig.Emit (OpCodes.Br, l);
		}
	}

	public class LabeledStatement : Statement {
		public readonly Location Location;
		bool defined;
		bool referenced;
		Label label;
		ILGenerator ig;

		FlowBranching.UsageVector vectors;
		
		public LabeledStatement (string label_name, Location l)
		{
			this.Location = l;
		}

		public Label LabelTarget (EmitContext ec)
		{
			if (defined)
				return label;
			ig = ec.ig;
			label = ec.ig.DefineLabel ();
			defined = true;

			return label;
		}

		public bool IsDefined {
			get {
				return defined;
			}
		}

		public bool HasBeenReferenced {
			get {
				return referenced;
			}
		}

		public void AddUsageVector (FlowBranching.UsageVector vector)
		{
			vector = vector.Clone ();
			vector.Next = vectors;
			vectors = vector;
		}

		public override bool Resolve (EmitContext ec)
		{
			ec.CurrentBranching.Label (vectors);

			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (ig != null && ig != ec.ig) {
				Report.Error (1632, "Control cannot leave body of anonymous method");
				return;
			}
			LabelTarget (ec);
			ec.ig.MarkLabel (label);
		}

		public void AddReference ()
		{
			referenced = true;
		}
	}
	

	/// <summary>
	///   `goto default' statement
	/// </summary>
	public class GotoDefault : Statement {
		
		public GotoDefault (Location l)
		{
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (ec.Switch == null){
				Report.Error (153, loc, "goto default is only valid in a switch statement");
				return;
			}

			if (!ec.Switch.GotDefault){
				Report.Error (159, loc, "No default target on switch statement");
				return;
			}
			ec.ig.Emit (OpCodes.Br, ec.Switch.DefaultTarget);
		}
	}

	/// <summary>
	///   `goto case' statement
	/// </summary>
	public class GotoCase : Statement {
		Expression expr;
		SwitchLabel sl;
		
		public GotoCase (Expression e, Location l)
		{
			expr = e;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			if (ec.Switch == null){
				Report.Error (153, loc, "goto case is only valid in a switch statement");
				return false;
			}

			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			if (!(expr is Constant)){
				Report.Error (159, loc, "Target expression for goto case is not constant");
				return false;
			}

			object val = Expression.ConvertIntLiteral (
				(Constant) expr, ec.Switch.SwitchType, loc);

			if (val == null)
				return false;
					
			sl = (SwitchLabel) ec.Switch.Elements [val];

			if (sl == null){
				Report.Error (
					159, loc,
					"No such label 'case " + val + "': for the goto case");
				return false;
			}

			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Br, sl.GetILLabelCode (ec));
		}
	}
	
	public class Throw : Statement {
		Expression expr;
		
		public Throw (Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			ec.CurrentBranching.CurrentUsageVector.Throw ();

			if (expr != null){
				expr = expr.Resolve (ec);
				if (expr == null)
					return false;

				ExprClass eclass = expr.eclass;

				if (!(eclass == ExprClass.Variable || eclass == ExprClass.PropertyAccess ||
					eclass == ExprClass.Value || eclass == ExprClass.IndexerAccess)) {
					expr.Error_UnexpectedKind ("value, variable, property or indexer access ", loc);
					return false;
				}

				Type t = expr.Type;
				
				if ((t != TypeManager.exception_type) &&
					!t.IsSubclassOf (TypeManager.exception_type) &&
					!(expr is NullLiteral)) {
					Error (155,
						"The type caught or thrown must be derived " +
						"from System.Exception");
					return false;
				}
				return true;
			}

			if (ec.CurrentBranching.InFinally (true)) {
				Error (724, "A throw statement with no argument is only allowed in a catch clause nested inside of the innermost catch clause");
				return false;
			}

			if (!ec.CurrentBranching.InCatch ()) {
				Error (156, "A throw statement with no argument is only allowed in a catch clause");
				return false;
			}
			return true;
		}
			
		protected override void DoEmit (EmitContext ec)
		{
			if (expr == null)
					ec.ig.Emit (OpCodes.Rethrow);
				else {
			expr.Emit (ec);

			ec.ig.Emit (OpCodes.Throw);
		}
	}
	}

	public class Break : Statement {
		
		public Break (Location l)
		{
			loc = l;
		}

		bool crossing_exc;

		public override bool Resolve (EmitContext ec)
		{
			if (!ec.CurrentBranching.InLoop () && !ec.CurrentBranching.InSwitch ()){
				Error (139, "No enclosing loop or switch to continue to");
				return false;
			} else if (ec.CurrentBranching.InFinally (false)) {
				Error (157, "Control can not leave the body of the finally block");
				return false;
			} else if (ec.CurrentBranching.InTryOrCatch (false))
				ec.CurrentBranching.AddFinallyVector (
					ec.CurrentBranching.CurrentUsageVector);
			else if (ec.CurrentBranching.InLoop ())
				ec.CurrentBranching.AddBreakVector (
					ec.CurrentBranching.CurrentUsageVector);

			crossing_exc = ec.CurrentBranching.BreakCrossesTryCatchBoundary ();

			if (!crossing_exc)
				ec.NeedReturnLabel ();

			ec.CurrentBranching.CurrentUsageVector.Break ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (crossing_exc)
				ig.Emit (OpCodes.Leave, ec.LoopEnd);
			else {
				ig.Emit (OpCodes.Br, ec.LoopEnd);
		}
	}
	}

	public class Continue : Statement {
		
		public Continue (Location l)
		{
			loc = l;
		}

		bool crossing_exc;

		public override bool Resolve (EmitContext ec)
		{
			if (!ec.CurrentBranching.InLoop () && !ec.CurrentBranching.InSwitch ()){
				Error (139, "No enclosing loop to continue to");
				return false;
			} else if (ec.CurrentBranching.InFinally (false)) {
				Error (157, "Control can not leave the body of the finally block");
				return false;
			} else if (ec.CurrentBranching.InTryOrCatch (false))
				ec.CurrentBranching.AddFinallyVector (ec.CurrentBranching.CurrentUsageVector);

			crossing_exc = ec.CurrentBranching.BreakCrossesTryCatchBoundary ();

			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			Label begin = ec.LoopBegin;
			
			if (crossing_exc)
				ec.ig.Emit (OpCodes.Leave, begin);
			else
				ec.ig.Emit (OpCodes.Br, begin);
		}
	}

	public abstract class VBLoopControlStatement : Statement {
		protected readonly VBLoopKind loop_kind;
		protected bool crossing_exc;

		protected VBLoopControlStatement (VBLoopKind kind, Location l)
		{
			loop_kind = kind;
			loc = l;
		}

		static FlowBranchingLoop FindInnermostLoop (FlowBranching branching)
		{
			for (; branching != null; branching = branching.Parent) {
				FlowBranchingLoop loop = branching as FlowBranchingLoop;
				if (loop != null)
					return loop;
			}

			return null;
		}

		protected FlowBranchingLoop FindTargetLoop (EmitContext ec)
		{
			for (FlowBranching branching = ec.CurrentBranching; branching != null; branching = branching.Parent) {
				FlowBranchingLoop loop = branching as FlowBranchingLoop;
				if (loop == null || loop.Kind != loop_kind)
					continue;

				return loop;
			}

			return null;
		}

		protected bool CrossesTryCatchBoundary (EmitContext ec)
		{
			for (FlowBranching branching = ec.CurrentBranching; branching != null; branching = branching.Parent) {
				FlowBranchingLoop loop = branching as FlowBranchingLoop;
				if (loop != null && loop.Kind == loop_kind)
					return false;

				if (branching.Type == FlowBranching.BranchingType.Exception)
					return true;
			}

			return false;
		}

		protected bool IsImmediateLoopTarget (EmitContext ec)
		{
			FlowBranchingLoop innermost = FindInnermostLoop (ec.CurrentBranching);
			return innermost != null && innermost.Kind == loop_kind;
		}

		protected string LoopKeyword {
			get { return loop_kind.ToString (); }
		}
	}

	public class VBExitLoop : VBLoopControlStatement {
		public VBExitLoop (VBLoopKind kind, Location l)
			: base (kind, l)
		{
		}

		public override bool Resolve (EmitContext ec)
		{
			FlowBranchingLoop target = FindTargetLoop (ec);
			if (target == null) {
				Error (139, "No enclosing `{0}' statement out of which to exit", LoopKeyword);
				return false;
			}

			if (ec.CurrentBranching.InFinally (false)) {
				Error (157, "Control can not leave the body of the finally block");
				return false;
			}

			crossing_exc = CrossesTryCatchBoundary (ec);
			if (crossing_exc)
				ec.CurrentBranching.AddFinallyVector (ec.CurrentBranching.CurrentUsageVector);
			else
				target.AddBreakVector (ec.CurrentBranching.CurrentUsageVector);

			ec.NeedReturnLabel ();

			if (IsImmediateLoopTarget (ec))
				ec.CurrentBranching.CurrentUsageVector.Break ();
			else
				ec.CurrentBranching.CurrentUsageVector.Goto ();

			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			Label continue_label, exit_label;
			if (!ec.TryGetVBLoopLabels (loop_kind, out continue_label, out exit_label))
				throw new InvalidOperationException ();

			if (crossing_exc)
				ec.ig.Emit (OpCodes.Leave, exit_label);
			else
				ec.ig.Emit (OpCodes.Br, exit_label);
		}
	}

	public class VBContinueLoop : VBLoopControlStatement {
		public VBContinueLoop (VBLoopKind kind, Location l)
			: base (kind, l)
		{
		}

		public override bool Resolve (EmitContext ec)
		{
			if (FindTargetLoop (ec) == null) {
				Error (139, "No enclosing `{0}' statement to continue", LoopKeyword);
				return false;
			}

			if (ec.CurrentBranching.InFinally (false)) {
				Error (157, "Control can not leave the body of the finally block");
				return false;
			}

			crossing_exc = CrossesTryCatchBoundary (ec);
			if (crossing_exc)
				ec.CurrentBranching.AddFinallyVector (ec.CurrentBranching.CurrentUsageVector);

			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			Label continue_label, exit_label;
			if (!ec.TryGetVBLoopLabels (loop_kind, out continue_label, out exit_label))
				throw new InvalidOperationException ();

			if (crossing_exc)
				ec.ig.Emit (OpCodes.Leave, continue_label);
			else
				ec.ig.Emit (OpCodes.Br, continue_label);
		}
	}

	//
	// The information about a user-perceived local variable
	//
	public class LocalInfo {
		public Expression Type;

		//
		// Most of the time a variable will be stored in a LocalBuilder
		//
		// But sometimes, it will be stored in a field (variables that have been
		// hoisted by iterators or by anonymous methods).  The context of the field will
		// be stored in the EmitContext
		//
		//
		public LocalBuilder LocalBuilder;
		public FieldBuilder FieldBuilder;

		public Type VariableType;
		public readonly string Name;
		public readonly Location Location;
		public readonly Block Block;

		public VariableInfo VariableInfo;

 		enum Flags : byte {
 			Used = 1,
			ReadOnly = 2,
			Pinned = 4,
			IsThis = 8,
			Captured = 16,
			AddressTaken = 32
		}

		Flags flags;

		public LocalInfo (Expression type, string name, Block block, Location l)
		{
			Type = type;
			Name = name;
			Block = block;
			Location = l;
		}

		public LocalInfo (TypeContainer tc, Block block, Location l)
		{
			VariableType = tc.TypeBuilder;
			Block = block;
			Location = l;
		}

		public bool IsThisAssigned (EmitContext ec, Location loc)
		{
			if (VariableInfo == null)
				throw new Exception ();

			if (!ec.DoFlowAnalysis || ec.CurrentBranching.IsAssigned (VariableInfo))
				return true;

			return VariableInfo.TypeInfo.IsFullyInitialized (ec.CurrentBranching, VariableInfo, loc);
		}

		public bool IsAssigned (EmitContext ec)
		{
			if (VariableInfo == null)
				throw new Exception ();

			return !ec.DoFlowAnalysis || ec.CurrentBranching.IsAssigned (VariableInfo);
		}

		public bool Resolve (EmitContext ec)
		{
			if (VariableType == null) {
				TypeExpr texpr = Type.ResolveAsTypeTerminal (ec);
				if (texpr == null)
					return false;
				
				VariableType = texpr.Type;
			}

			if (VariableType == TypeManager.void_type) {
				Report.Error (1547, Location,
					      "Keyword 'void' cannot be used in this context");
				return false;
			}

			if (VariableType.IsAbstract && VariableType.IsSealed) {
				Report.Error (723, Location, "Cannot declare variable of static type '{0}'", TypeManager.CSharpName (VariableType));
				return false;
			}
// TODO: breaks the build
//			if (VariableType.IsPointer && !ec.InUnsafe)
//				Expression.UnsafeError (Location);

			return true;
		}

		//
		// Whether the variable is Fixed (because its Pinned or its a value type)
		//
		public bool IsFixed {
			get {
				if (((flags & Flags.Pinned) != 0) || TypeManager.IsValueType (VariableType))
 					return true;
				
 				return false;
			}
		}
		
		public bool IsCaptured {
			get {
				return (flags & Flags.Captured) != 0;
			}

			set {
				flags |= Flags.Captured;
			}
		}

		public bool AddressTaken {
			get {
				return (flags & Flags.AddressTaken) != 0;
			}

			set {
				flags |= Flags.AddressTaken;
			}
		}

		public override string ToString ()
		{
			return String.Format ("LocalInfo ({0},{1},{2},{3})",
					      Name, Type, VariableInfo, Location);
		}

		public bool Used {
			get {
				return (flags & Flags.Used) != 0;
			}
			set {
				flags = value ? (flags | Flags.Used) : (unchecked (flags & ~Flags.Used));
			}
		}

		public bool ReadOnly {
			get {
				return (flags & Flags.ReadOnly) != 0;
			}
			set {
				flags = value ? (flags | Flags.ReadOnly) : (unchecked (flags & ~Flags.ReadOnly));
			}
		}

		//
		// Whether the variable is pinned, if Pinned the variable has been 
		// allocated in a pinned slot with DeclareLocal.
		//
		public bool Pinned {
			get {
				return (flags & Flags.Pinned) != 0;
			}
			set {
				flags = value ? (flags | Flags.Pinned) : (flags & ~Flags.Pinned);
			}
		}

		public bool IsThis {
			get {
				return (flags & Flags.IsThis) != 0;
			}
			set {
				flags = value ? (flags | Flags.IsThis) : (flags & ~Flags.IsThis);
			}
		}
	}
		
	/// <summary>
	///   Block represents a C# block.
	/// </summary>
	///
	/// <remarks>
	///   This class is used in a number of places: either to represent
	///   explicit blocks that the programmer places or implicit blocks.
	///
	///   Implicit blocks are used as labels or to introduce variable
	///   declarations.
	///
	///   Top-level blocks derive from Block, and they are called ToplevelBlock
	///   they contain extra information that is not necessary on normal blocks.
	/// </remarks>
	public class Block : Statement {
		public Block    Parent;
		public readonly Location  StartLocation;
		public Location           EndLocation = Location.Null;

		[Flags]
		public enum Flags {
			Implicit  = 1,
			Unchecked = 2,
			BlockUsed = 4,
			VariablesInitialized = 8,
			HasRet = 16,
			IsDestructor = 32,
			HasVarargs = 64,
			IsToplevel = 128,
			Unsafe = 256
		}
		Flags flags;

		public bool Implicit {
			get {
				return (flags & Flags.Implicit) != 0;
			}
		}

		public bool Unchecked {
			get {
				return (flags & Flags.Unchecked) != 0;
			}
			set {
				flags |= Flags.Unchecked;
			}
		}

		public bool Unsafe {
			get {
				return (flags & Flags.Unsafe) != 0;
			}
			set {
				flags |= Flags.Unsafe;
			}
		}

		public bool HasVarargs {
			get {
				if (Parent != null)
					return Parent.HasVarargs;
				else
					return (flags & Flags.HasVarargs) != 0;
			}
			set {
				flags |= Flags.HasVarargs;
			}
		}

		//
		// The statements in this block
		//
		public ArrayList statements;
		int num_statements;

		//
		// An array of Blocks.  We keep track of children just
		// to generate the local variable declarations.
		//
		// Statements and child statements are handled through the
		// statements.
		//
		ArrayList children;
		
		//
		// Labels.  (label, block) pairs.
		//
		Hashtable labels;

		//
		// Keeps track of (name, type) pairs
		//
		Hashtable variables;

		//
		// Keeps track of constants
		Hashtable constants;

		//
		// The parameters for the block, this is only needed on the toplevel block really
		// TODO: move `parameters' into ToplevelBlock
		Parameters parameters;
		
		//
		// If this is a switch section, the enclosing switch block.
		//
		Block switch_block;

		protected static int id;

		int this_id;
		
		public Block (Block parent)
			: this (parent, (Flags) 0, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Flags flags)
			: this (parent, flags, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Flags flags, Parameters parameters)
			: this (parent, flags, parameters, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Location start, Location end)
			: this (parent, (Flags) 0, start, end)
		{ }

		public Block (Block parent, Parameters parameters, Location start, Location end)
			: this (parent, (Flags) 0, parameters, start, end)
		{ }

		public Block (Block parent, Flags flags, Location start, Location end)
			: this (parent, flags, Parameters.EmptyReadOnlyParameters, start, end)
		{ }

		public Block (Block parent, Flags flags, Parameters parameters,
			      Location start, Location end)
		{
			if (parent != null)
				parent.AddChild (this);
			
			this.Parent = parent;
			this.flags = flags;
			this.parameters = parameters;
			this.StartLocation = start;
			this.EndLocation = end;
			this.loc = start;
			this_id = id++;
			statements = new ArrayList ();

			if (parent != null && Implicit) {
				if (parent.child_variable_names == null)
					parent.child_variable_names = new Hashtable (StringComparer.OrdinalIgnoreCase);
				// share with parent
				child_variable_names = parent.child_variable_names;
			}
				
		}

		public Block CreateSwitchBlock (Location start)
		{
			Block new_block = new Block (this, start, start);
			new_block.switch_block = this;
			return new_block;
		}

		public int ID {
			get {
				return this_id;
			}
		}

		void AddChild (Block b)
		{
			if (children == null)
				children = new ArrayList ();
			
			children.Add (b);
		}

		public void SetEndLocation (Location loc)
		{
			EndLocation = loc;
		}

		/// <summary>
		///   Adds a label to the current block. 
		/// </summary>
		///
		/// <returns>
		///   false if the name already exists in this block. true
		///   otherwise.
		/// </returns>
		///
		public bool AddLabel (string name, LabeledStatement target, Location loc)
		{
			if (switch_block != null)
				return switch_block.AddLabel (name, target, loc);

			Block cur = this;
			while (cur != null) {
				if (cur.DoLookupLabel (name) != null) {
					Report.Error (
						140, loc, "The label '{0}' is a duplicate",
						name);
					return false;
				}

				if (!Implicit)
					break;

				cur = cur.Parent;
			}

			while (cur != null) {
				if (cur.DoLookupLabel (name) != null) {
					Report.Error (
						158, loc,
						"The label '{0}' shadows another label " +
						"by the same name in a containing scope.",
						name);
					return false;
				}

				if (children != null) {
					foreach (Block b in children) {
						LabeledStatement s = b.DoLookupLabel (name);
						if (s == null)
							continue;

						Report.Error (
							158, s.Location,
							"The label '{0}' shadows another " +
							"label by the same name in a " +
							"containing scope.",
							name);
						return false;
					}
				}


				cur = cur.Parent;
			}

			if (labels == null)
				labels = new Hashtable (StringComparer.OrdinalIgnoreCase);

			labels.Add (name, target);
			return true;
		}

		public LabeledStatement LookupLabel (string name)
		{
			LabeledStatement s = DoLookupLabel (name);
			if (s != null)
				return s;

			if (children == null)
				return null;

			foreach (Block child in children) {
				if (!child.Implicit)
					continue;

				s = child.LookupLabel (name);
				if (s != null)
					return s;
			}

			return null;
		}

		LabeledStatement DoLookupLabel (string name)
		{
			if (switch_block != null)
				return switch_block.LookupLabel (name);

			if (labels != null)
				if (labels.Contains (name))
					return ((LabeledStatement) labels [name]);

			return null;
		}

		Hashtable child_variable_names;

		// <summary>
		//   Marks a variable with name @name as being used in a child block.
		//   If a variable name has been used in a child block, it's illegal to
		//   declare a variable with the same name in the current block.
		// </summary>
		public void AddChildVariableName (string name)
		{
			if (child_variable_names == null)
				child_variable_names = new Hashtable (StringComparer.OrdinalIgnoreCase);

			child_variable_names [name] = null;
		}

		// <summary>
		//   Checks whether a variable name has already been used in a child block.
		// </summary>
		public bool IsVariableNameUsedInChildBlock (string name)
		{
			if (child_variable_names == null)
				return false;

			return child_variable_names.Contains (name);
		}

		public LocalInfo AddVariable (Expression type, string name, Parameters pars, Location l)
		{
			if (variables == null)
				variables = new Hashtable (StringComparer.OrdinalIgnoreCase);

			LocalInfo vi = GetLocalInfo (name);
			if (vi != null) {
				if (vi.Block != this)
					Report.Error (136, l, "A local variable named `" + name + "' " +
						      "cannot be declared in this scope since it would " +
						      "give a different meaning to `" + name + "', which " +
						      "is already used in a `parent or current' scope to " +
						      "denote something else");
				else
					Report.Error (128, l, "A local variable `" + name + "' is already " +
						      "defined in this scope");
				return null;
			}

			if (IsVariableNameUsedInChildBlock (name)) {
				Report.Error (136, l, "A local variable named `" + name + "' " +
					      "cannot be declared in this scope since it would " +
					      "give a different meaning to `" + name + "', which " +
					      "is already used in a `child' scope to denote something " +
					      "else");
				return null;
			}

			if (pars != null) {
				int idx;
				Parameter p = pars.GetParameterByName (name, out idx);
				if (p != null) {
					Report.Error (136, l, "A local variable named `" + name + "' " +
						      "cannot be declared in this scope since it would " +
						      "give a different meaning to `" + name + "', which " +
						      "is already used in a `parent or current' scope to " +
						      "denote something else");
					return null;
				}
			}

			vi = new LocalInfo (type, name, this, l);

			variables.Add (name, vi);

			// Mark 'name' as "used by a child block" in every surrounding block
			Block cur = this;
			while (cur != null && cur.Implicit) 
				cur = cur.Parent;
			if (cur != null)
				for (Block par = cur.Parent; par != null; par = par.Parent)
					par.AddChildVariableName (name);

			if ((flags & Flags.VariablesInitialized) != 0)
				throw new Exception ();

			// Console.WriteLine ("Adding {0} to {1}", name, ID);
			return vi;
		}

		public bool AddConstant (Expression type, string name, Expression value, Parameters pars, Location l)
		{
			if (AddVariable (type, name, pars, l) == null)
				return false;
			
			if (constants == null)
				constants = new Hashtable (StringComparer.OrdinalIgnoreCase);

			constants.Add (name, value);
			return true;
		}

		public Hashtable Variables {
			get {
				return variables;
			}
		}

		public LocalInfo GetLocalInfo (string name)
		{
			for (Block b = this; b != null; b = b.Parent) {
				if (b.variables != null) {
					LocalInfo ret = b.variables [name] as LocalInfo;
					if (ret != null)
						return ret;
				}
			}
			return null;
		}

		public Expression GetVariableType (string name)
		{
			LocalInfo vi = GetLocalInfo (name);

			if (vi != null)
				return vi.Type;

			return null;
		}

		public Expression GetConstantExpression (string name)
		{
			for (Block b = this; b != null; b = b.Parent) {
				if (b.constants != null) {
					Expression ret = b.constants [name] as Expression;
					if (ret != null)
						return ret;
				}
			}
			return null;
		}
		
		/// <summary>
		///   True if the variable named @name is a constant
		///  </summary>
		public bool IsConstant (string name)
		{
			Expression e = null;
			
			e = GetConstantExpression (name);
			
			return e != null;
		}
		
		//
		// Returns a `ParameterReference' for the given name, or null if there
		// is no such parameter
		//
		public ParameterReference GetParameterReference (string name, Location loc)
		{
				Block b = this;

			do {
				Parameters pars = b.parameters;
				
				if (pars != null){
					Parameter par;
					int idx;
					
					par = pars.GetParameterByName (name, out idx);
					if (par != null){
						ParameterReference pr;

						pr = new ParameterReference (pars, this, idx, name, loc);
						return pr;
					}
				}
					b = b.Parent;
			} while (b != null);
			return null;
			}

		//
		// Whether the parameter named `name' is local to this block, 
		// or false, if the parameter belongs to an encompassing block.
		//
		public bool IsLocalParameter (string name)
		{
				Block b = this;
			int toplevel_count = 0;

			do {
				if (this is ToplevelBlock)
					toplevel_count++;

				Parameters pars = b.parameters;
				if (pars != null){
					if (pars.GetParameterByName (name) != null)
						return true;
					return false;
				}
				if (toplevel_count > 0)
					return false;
					b = b.Parent;
			} while (b != null);
			return false;
			}
		
		//
		// Whether the `name' is a parameter reference
		//
		public bool IsParameterReference (string name)
		{
			Block b = this;

			do {
				Parameters pars = b.parameters;
				
				if (pars != null)
					if (pars.GetParameterByName (name) != null)
						return true;
				b = b.Parent;
			} while (b != null);
			return false;
		}

		/// <returns>
		///   A list of labels that were not used within this block
		/// </returns>
		public string [] GetUnreferenced ()
		{
			// FIXME: Implement me
			return null;
		}

		public void AddStatement (Statement s)
		{
			statements.Add (s);
			flags |= Flags.BlockUsed;
		}

		public bool Used {
			get {
				return (flags & Flags.BlockUsed) != 0;
			}
		}

		public void Use ()
		{
			flags |= Flags.BlockUsed;
		}

		public bool HasRet {
			get {
				return (flags & Flags.HasRet) != 0;
			}
		}

		public bool IsDestructor {
			get {
				return (flags & Flags.IsDestructor) != 0;
			}
		}

		public void SetDestructor ()
		{
			flags |= Flags.IsDestructor;
		}

		VariableMap param_map, local_map;

		public VariableMap ParameterMap {
			get {
				if ((flags & Flags.VariablesInitialized) == 0)
					throw new Exception ("Variables have not been initialized yet");

				return param_map;
			}
		}

		public VariableMap LocalMap {
			get {
				if ((flags & Flags.VariablesInitialized) == 0)
					throw new Exception ("Variables have not been initialized yet");

				return local_map;
			}
		}

		/// <summary>
		///   Emits the variable declarations and labels.
		/// </summary>
		/// <remarks>
		///   tc: is our typecontainer (to resolve type references)
		///   ig: is the code generator:
		/// </remarks>
		public void ResolveMeta (ToplevelBlock toplevel, EmitContext ec, InternalParameters ip)
		{
			bool old_unsafe = ec.InUnsafe;

			// If some parent block was unsafe, we remain unsafe even if this block
			// isn't explicitly marked as such.
			ec.InUnsafe |= Unsafe;

			//
			// Compute the VariableMap's.
			//
			// Unfortunately, we don't know the type when adding variables with
			// AddVariable(), so we need to compute this info here.
			//

			LocalInfo[] locals;
			if (variables != null) {
				foreach (LocalInfo li in variables.Values)
					li.Resolve (ec);

				locals = new LocalInfo [variables.Count];
				variables.Values.CopyTo (locals, 0);
			} else
				locals = new LocalInfo [0];

			if (Parent != null)
				local_map = new VariableMap (Parent.LocalMap, locals);
			else
				local_map = new VariableMap (locals);

			param_map = new VariableMap (ip);
			flags |= Flags.VariablesInitialized;

			bool old_check_state = ec.ConstantCheckState;
			ec.ConstantCheckState = (flags & Flags.Unchecked) == 0;
				
			//
			// Process this block variables
			//
			if (variables != null){
				foreach (DictionaryEntry de in variables){
					string name = (string) de.Key;
					LocalInfo vi = (LocalInfo) de.Value;
					
					if (vi.VariableType == null)
						continue;

					Type variable_type = vi.VariableType;

					if (variable_type.IsPointer){
						//
						// Am not really convinced that this test is required (Microsoft does it)
						// but the fact is that you would not be able to use the pointer variable
						// *anyways*
						//
						if (!TypeManager.VerifyUnManaged (TypeManager.GetElementType (variable_type),
                                                                                  vi.Location))
							continue;
					}

#if false
					if (remap_locals)
						vi.FieldBuilder = ec.MapVariable (name, vi.VariableType);
					else if (vi.Pinned)
						//
						// This is needed to compile on both .NET 1.x and .NET 2.x
						// the later introduced `DeclareLocal (Type t, bool pinned)'
						//
						vi.LocalBuilder = TypeManager.DeclareLocalPinned (ig, vi.VariableType);
					else if (!vi.IsThis)
						vi.LocalBuilder = ig.DeclareLocal (vi.VariableType);
#endif

					if (constants == null)
						continue;

					Expression cv = (Expression) constants [name];
					if (cv == null)
						continue;

					ec.CurrentBlock = this;
					Expression e = cv.Resolve (ec);
					if (e == null)
						continue;

					Constant ce = e as Constant;
					if (ce == null){
						Report.Error (133, vi.Location,
							      "The expression being assigned to `" +
							      name + "' must be constant (" + e + ")");
						continue;
					}

					if (e.Type != variable_type){
						e = Const.ChangeType (vi.Location, ce, variable_type);
						if (e == null)
							continue;
					}

					constants.Remove (name);
					constants.Add (name, e);
				}
			}
			ec.ConstantCheckState = old_check_state;

			//
			// Now, handle the children
			//
			if (children != null){
				foreach (Block b in children)
					b.ResolveMeta (toplevel, ec, ip);
			}
			ec.InUnsafe = old_unsafe;
		}

		//
		// Emits the local variable declarations for a block
		//
		public void EmitMeta (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			if (variables != null){
				bool have_captured_vars = ec.HaveCapturedVariables ();
				bool remap_locals = ec.RemapToProxy;
				
				foreach (DictionaryEntry de in variables){
					LocalInfo vi = (LocalInfo) de.Value;

					if (have_captured_vars && ec.IsCaptured (vi))
						continue;

					if (remap_locals){
						vi.FieldBuilder = ec.MapVariable (vi.Name, vi.VariableType);
					} else {
						if (vi.Pinned)
							//
							// This is needed to compile on both .NET 1.x and .NET 2.x
							// the later introduced `DeclareLocal (Type t, bool pinned)'
							//
							vi.LocalBuilder = TypeManager.DeclareLocalPinned (ig, vi.VariableType);
						else if (!vi.IsThis)
							vi.LocalBuilder = ig.DeclareLocal (vi.VariableType);
					}
				}
			}

			if (children != null){
				foreach (Block b in children)
					b.EmitMeta (ec);
			}
		}

		void UsageWarning (FlowBranching.UsageVector vector)
		{
			string name;
			
			if ((variables != null) && (RootContext.WarningLevel >= 3)) {
				foreach (DictionaryEntry de in variables){
					LocalInfo vi = (LocalInfo) de.Value;
					
					if (vi.Used)
						continue;
					
					name = (string) de.Key;
						
					if (vector.IsAssigned (vi.VariableInfo)){
						Report.Warning (219, vi.Location, "The variable '{0}' is assigned but its value is never used", name);
					} else {
						Report.Warning (168, vi.Location, "The variable '{0}' is declared but never used", name);
					}
				}
			}
		}

		bool unreachable_shown;

		public override bool Resolve (EmitContext ec)
		{
			Block prev_block = ec.CurrentBlock;
			bool ok = true;

			int errors = Report.Errors;

			ec.CurrentBlock = this;
			ec.StartFlowBranching (this);

			Report.Debug (4, "RESOLVE BLOCK", StartLocation, ec.CurrentBranching);

			bool unreachable = unreachable_shown;

			int statement_count = statements.Count;
			for (int ix = 0; ix < statement_count; ix++){
				Statement s = (Statement) statements [ix];
				
				if (unreachable && !(s is LabeledStatement)) {
					if (s == EmptyStatement.Value)
						s.loc = EndLocation;

					if (!s.ResolveUnreachable (ec, !unreachable_shown))
						ok = false;

					if (s != EmptyStatement.Value)
						unreachable_shown = true;
					else
						s.loc = Location.Null;

					if (ok && !(s is Block)) {
						statements [ix] = EmptyStatement.Value;
						continue;
					}
				}

				if (s.Resolve (ec) == false) {
 					ok = false;
					statements [ix] = EmptyStatement.Value;
					continue;
				}

				num_statements = ix + 1;

				if (s is LabeledStatement)
					unreachable = false;
				else
					unreachable = ec.CurrentBranching.CurrentUsageVector.Reachability.IsUnreachable;
			}

			Report.Debug (4, "RESOLVE BLOCK DONE", StartLocation,
				      ec.CurrentBranching, statement_count, num_statements);


			FlowBranching.UsageVector vector = ec.DoEndFlowBranching ();

			ec.CurrentBlock = prev_block;

			if ((labels != null) && (RootContext.WarningLevel >= 2)) {
				foreach (LabeledStatement label in labels.Values)
					if (!label.HasBeenReferenced)
						Report.Warning (164, label.Location,
								"This label has not been referenced");
			}

			Report.Debug (4, "RESOLVE BLOCK DONE #2", StartLocation, vector);

			if ((vector.Reachability.Returns == FlowBranching.FlowReturns.Always) ||
			    (vector.Reachability.Throws == FlowBranching.FlowReturns.Always) ||
			    (vector.Reachability.Reachable == FlowBranching.FlowReturns.Never))
				flags |= Flags.HasRet;

			if (ok && (errors == Report.Errors)) {
				if (RootContext.WarningLevel >= 3)
					UsageWarning (vector);
			}

			return ok;
		}
		
		public override bool ResolveUnreachable (EmitContext ec, bool warn)
		{
			unreachable_shown = true;

			if (warn && (RootContext.WarningLevel >= 2))
				Report.Warning (162, loc, "Unreachable code detected");

			if (Implicit)
				return Resolve (ec);

			ec.StartFlowBranching (FlowBranching.BranchingType.Block, loc);
			bool ok = Resolve (ec);
			ec.KillFlowBranching ();

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			for (int ix = 0; ix < num_statements; ix++){
				Statement s = (Statement) statements [ix];

				// Check whether we are the last statement in a
				// top-level block.

				if (((Parent == null) || Implicit) && (ix+1 == num_statements) && !(s is Block))
					ec.IsLastStatement = true;
				else
					ec.IsLastStatement = false;

				s.Emit (ec);
			}
		}

		public override void Emit (EmitContext ec)
		{
			Block prev_block = ec.CurrentBlock;

			ec.CurrentBlock = this;

			bool emit_debug_info = (CodeGen.SymbolWriter != null);
			bool is_lexical_block = !Implicit && (Parent != null);

			if (emit_debug_info) {
				if (is_lexical_block)
					ec.ig.BeginScope ();

				if (variables != null) {
					foreach (DictionaryEntry de in variables) {
						string name = (string) de.Key;
						LocalInfo vi = (LocalInfo) de.Value;

						if (vi.LocalBuilder == null)
							continue;

						ec.DefineLocalVariable (name, vi.LocalBuilder);
					}
				}
			}

			ec.Mark (StartLocation, true);
			DoEmit (ec);
			ec.Mark (EndLocation, true); 

			if (emit_debug_info && is_lexical_block)
				ec.ig.EndScope ();

			ec.CurrentBlock = prev_block;
		}

		public ToplevelBlock Toplevel {
			get {
				Block b = this;
				while (b.Parent != null){
					if ((b.flags & Flags.IsToplevel) != 0)
						break;
					b = b.Parent;
				}

				return (ToplevelBlock) b;
			}
	}

	//
		// Returns true if we ar ea child of `b'.
		//
		public bool IsChildOf (Block b)
		{
			Block current = this;
			
			do {
				if (current.Parent == b)
					return true;
				current = current.Parent;
			} while (current != null);
			return false;
		}
	}

	//
	// A toplevel block contains extra information, the split is done
	// only to separate information that would otherwise bloat the more
	// lightweight Block.
	//
	// In particular, this was introduced when the support for Anonymous
	// Methods was implemented. 
	// 
	public class ToplevelBlock : Block {
		//
		// Pointer to the host of this anonymous method, or null
		// if we are the topmost block
		//
		public ToplevelBlock Container;
		CaptureContext capture_context;

		Hashtable capture_contexts;

		static int did = 0;
		
			
		public void RegisterCaptureContext (CaptureContext cc)
		{
			if (capture_contexts == null)
				capture_contexts = new Hashtable ();
			capture_contexts [cc] = cc;
		}

		public void CompleteContexts ()
		{
			if (capture_contexts == null)
				return;

			foreach (CaptureContext cc in capture_contexts.Keys){
				cc.AdjustScopes ();
			}
		}
		
		public CaptureContext ToplevelBlockCaptureContext {
			get {
				return capture_context;
			}
		}
		
		//
		// Parent is only used by anonymous blocks to link back to their
		// parents
		//
		public ToplevelBlock (ToplevelBlock container, Parameters parameters, Location start) :
			base (null, Flags.IsToplevel, parameters, start, Location.Null)
		{
			Container = container;
		}
		
		public ToplevelBlock (Parameters parameters, Location start) :
			base (null, Flags.IsToplevel, parameters, start, Location.Null)
		{
		}

		public ToplevelBlock (Flags flags, Parameters parameters, Location start) :
			base (null, flags | Flags.IsToplevel, parameters, start, Location.Null)
		{
		}

		public ToplevelBlock (Location loc) : base (null, Flags.IsToplevel, loc, loc)
		{
		}

		public void SetHaveAnonymousMethods (Location loc, AnonymousMethod host)
		{
			if (capture_context == null)
				capture_context = new CaptureContext (this, loc, host);
		}

		public CaptureContext CaptureContext {
			get {
				return capture_context;
			}
		}
	}
	
	public class SwitchLabel {
		Expression label;
		object converted;
		public Location loc;

		Label il_label;
		bool  il_label_set;
		Label il_label_code;
		bool  il_label_code_set;

		//
		// if expr == null, then it is the default case.
		//
		public SwitchLabel (Expression expr, Location l)
		{
			label = expr;
			loc = l;
		}

		public Expression Label {
			get {
				return label;
			}
		}

		public object Converted {
			get {
				return converted;
			}
		}

		public Label GetILLabel (EmitContext ec)
		{
			if (!il_label_set){
				il_label = ec.ig.DefineLabel ();
				il_label_set = true;
			}
			return il_label;
		}

		public Label GetILLabelCode (EmitContext ec)
		{
			if (!il_label_code_set){
				il_label_code = ec.ig.DefineLabel ();
				il_label_code_set = true;
			}
			return il_label_code;
		}				
		
		//
		// Resolves the expression, reduces it to a literal if possible
		// and then converts it to the requested type.
		//
		public bool ResolveAndReduce (EmitContext ec, Type required_type)
		{
			if (label == null)
				return true;
			
			Expression e = label.Resolve (ec);

			if (e == null)
				return false;

			if (!(e is Constant)){
				Report.Error (150, loc, "A constant value is expected, got: " + e);
				return false;
			}

			if (e is StringConstant || e is NullLiteral){
				if (required_type == TypeManager.string_type){
					converted = e;
					return true;
				}
			}

			converted = Expression.ConvertIntLiteral ((Constant) e, required_type, loc);
			if (converted == null)
				return false;

			return true;
		}
	}

	public class SwitchSection {
		// An array of SwitchLabels.
		public readonly ArrayList Labels;
		public readonly Block Block;
		
		public SwitchSection (ArrayList labels, Block block)
		{
			Labels = labels;
			Block = block;
		}
	}
	
	public class Switch : Statement {
		public readonly ArrayList Sections;
		public Expression Expr;

		/// <summary>
		///   Maps constants whose type type SwitchType to their  SwitchLabels.
		/// </summary>
		public Hashtable Elements;

		/// <summary>
		///   The governing switch type
		/// </summary>
		public Type SwitchType;

		//
		// Computed
		//
		bool got_default;
		Label default_target;
		Expression new_expr;
		bool is_constant;
		SwitchSection constant_section;

		//
		// The types allowed to be implicitly cast from
		// on the governing type
		//
		static Type [] allowed_types;
		
		public Switch (Expression e, ArrayList sects, Location l)
		{
			Expr = e;
			Sections = sects;
			loc = l;
		}

		public bool GotDefault {
			get {
				return got_default;
			}
		}

		public Label DefaultTarget {
			get {
				return default_target;
			}
		}

		//
		// Determines the governing type for a switch.  The returned
		// expression might be the expression from the switch, or an
		// expression that includes any potential conversions to the
		// integral types or to string.
		//
		Expression SwitchGoverningType (EmitContext ec, Type t)
		{
			if (t == TypeManager.int32_type ||
			    t == TypeManager.uint32_type ||
			    t == TypeManager.char_type ||
			    t == TypeManager.byte_type ||
			    t == TypeManager.sbyte_type ||
			    t == TypeManager.ushort_type ||
			    t == TypeManager.short_type ||
			    t == TypeManager.uint64_type ||
			    t == TypeManager.int64_type ||
			    t == TypeManager.string_type ||
				t == TypeManager.bool_type ||
				t.IsSubclassOf (TypeManager.enum_type))
				return Expr;

			if (allowed_types == null){
				allowed_types = new Type [] {
					TypeManager.int32_type,
					TypeManager.uint32_type,
					TypeManager.sbyte_type,
					TypeManager.byte_type,
					TypeManager.short_type,
					TypeManager.ushort_type,
					TypeManager.int64_type,
					TypeManager.uint64_type,
					TypeManager.char_type,
					TypeManager.bool_type,
					TypeManager.string_type
				};
			}

			//
			// Try to find a *user* defined implicit conversion.
			//
			// If there is no implicit conversion, or if there are multiple
			// conversions, we have to report an error
			//
			Expression converted = null;

			//
			// VB.NET has no notion of User defined conversions
			//

// 			foreach (Type tt in allowed_types){
// 				Expression e;
				
// 				e = Convert.ImplicitUserConversion (ec, Expr, tt, loc);
// 				if (e == null)
// 					continue;

// 				//
// 				// Ignore over-worked ImplicitUserConversions that do
// 				// an implicit conversion in addition to the user conversion.
// 				// 
// 				if (e is UserCast){
// 					UserCast ue = e as UserCast;

// 					if (ue.Source != Expr)
// 						e = null;
// 				}
				
// 				if (converted != null){
// 					Report.ExtraInformation (
// 						loc,
// 						String.Format ("reason: more than one conversion to an integral type exist for type {0}",
// 							       TypeManager.CSharpName (Expr.Type)));
// 					return null;
// 				} else {
// 					converted = e;
// 				}
// 			}
			return converted;
		}

		static string Error152 {
			get {
				return "The label '{0}:' already occurs in this switch statement";
			}
		}
		
		//
		// Performs the basic sanity checks on the switch statement
		// (looks for duplicate keys and non-constant expressions).
		//
		// It also returns a hashtable with the keys that we will later
		// use to compute the switch tables
		//
		bool CheckSwitch (EmitContext ec)
		{
			Type compare_type;
			bool error = false;
			Elements = new Hashtable ();
				
			got_default = false;

			if (TypeManager.IsEnumType (SwitchType)){
				compare_type = TypeManager.EnumToUnderlying (SwitchType);
			} else
				compare_type = SwitchType;
			
			foreach (SwitchSection ss in Sections){
				foreach (SwitchLabel sl in ss.Labels){
					if (!sl.ResolveAndReduce (ec, SwitchType)){
						error = true;
						continue;
					}

					if (sl.Label == null){
						if (got_default){
							Report.Error (152, sl.loc, Error152, "default");
							error = true;
						}
						got_default = true;
						continue;
					}
					
					object key = sl.Converted;

					if (key is Constant)
						key = ((Constant) key).GetValue ();

					if (key == null)
						key = NullLiteral.Null;
					
					string lname = null;
					if (compare_type == TypeManager.uint64_type){
						ulong v = (ulong) key;

						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.int64_type){
						long v = (long) key;

						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.uint32_type){
						uint v = (uint) key;

						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.char_type){
						char v = (char) key;
						
						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.byte_type){
						byte v = (byte) key;
						
						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.sbyte_type){
						sbyte v = (sbyte) key;
						
						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.short_type){
						short v = (short) key;
						
						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.ushort_type){
						ushort v = (ushort) key;
						
						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.string_type){
						if (key is NullLiteral){
							if (Elements.Contains (NullLiteral.Null))
								lname = "null";
							else
								Elements.Add (NullLiteral.Null, null);
						} else {
							string s = (string) key;

							if (Elements.Contains (s))
								lname = s;
							else
								Elements.Add (s, sl);
						}
					} else if (compare_type == TypeManager.int32_type) {
						int v = (int) key;

						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.bool_type) {
						bool v = (bool) key;

						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					}
					else
					{
						throw new Exception ("Unknown switch type!" +
								     SwitchType + " " + compare_type);
					}

					if (lname != null){
						Report.Error (152, sl.loc, Error152, "case " + lname);
						error = true;
					}
				}
			}
			if (error)
				return false;
			
			return true;
		}

		void EmitObjectInteger (ILGenerator ig, object k)
		{
			if (k is int)
				IntConstant.EmitInt (ig, (int) k);
			else if (k is Constant) {
				EmitObjectInteger (ig, ((Constant) k).GetValue ());
			} 
			else if (k is uint)
				IntConstant.EmitInt (ig, unchecked ((int) (uint) k));
			else if (k is long)
			{
				if ((long) k >= int.MinValue && (long) k <= int.MaxValue)
				{
					IntConstant.EmitInt (ig, (int) (long) k);
					ig.Emit (OpCodes.Conv_I8);
				}
				else
					LongConstant.EmitLong (ig, (long) k);
			}
			else if (k is ulong)
			{
				if ((ulong) k < (1L<<32))
				{
					IntConstant.EmitInt (ig, (int) (long) k);
					ig.Emit (OpCodes.Conv_U8);
				}
				else
				{
					LongConstant.EmitLong (ig, unchecked ((long) (ulong) k));
				}
			}
			else if (k is char)
				IntConstant.EmitInt (ig, (int) ((char) k));
			else if (k is sbyte)
				IntConstant.EmitInt (ig, (int) ((sbyte) k));
			else if (k is byte)
				IntConstant.EmitInt (ig, (int) ((byte) k));
			else if (k is short)
				IntConstant.EmitInt (ig, (int) ((short) k));
			else if (k is ushort)
				IntConstant.EmitInt (ig, (int) ((ushort) k));
			else if (k is bool)
				IntConstant.EmitInt (ig, ((bool) k) ? 1 : 0);
			else
				throw new Exception ("Unhandled case");
		}
		
		// structure used to hold blocks of keys while calculating table switch
		class KeyBlock : IComparable
		{
			public KeyBlock (long _nFirst)
			{
				nFirst = nLast = _nFirst;
			}
			public long nFirst;
			public long nLast;
			public ArrayList rgKeys = null;
			// how many items are in the bucket
			public int Size = 1;
			public int Length
			{
				get { return (int) (nLast - nFirst + 1); }
			}
			public static long TotalLength (KeyBlock kbFirst, KeyBlock kbLast)
			{
				return kbLast.nLast - kbFirst.nFirst + 1;
			}
			public int CompareTo (object obj)
			{
				KeyBlock kb = (KeyBlock) obj;
				int nLength = Length;
				int nLengthOther = kb.Length;
				if (nLengthOther == nLength)
					return (int) (kb.nFirst - nFirst);
				return nLength - nLengthOther;
			}
		}

		/// <summary>
		/// This method emits code for a lookup-based switch statement (non-string)
		/// Basically it groups the cases into blocks that are at least half full,
		/// and then spits out individual lookup opcodes for each block.
		/// It emits the longest blocks first, and short blocks are just
		/// handled with direct compares.
		/// </summary>
		/// <param name="ec"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		void TableSwitchEmit (EmitContext ec, LocalBuilder val)
		{
			int cElements = Elements.Count;
			object [] rgKeys = new object [cElements];
			Elements.Keys.CopyTo (rgKeys, 0);
			Array.Sort (rgKeys);

			// initialize the block list with one element per key
			ArrayList rgKeyBlocks = new ArrayList ();
			foreach (object key in rgKeys)
				rgKeyBlocks.Add (new KeyBlock (System.Convert.ToInt64 (key)));

			KeyBlock kbCurr;
			// iteratively merge the blocks while they are at least half full
			// there's probably a really cool way to do this with a tree...
			while (rgKeyBlocks.Count > 1)
			{
				ArrayList rgKeyBlocksNew = new ArrayList ();
				kbCurr = (KeyBlock) rgKeyBlocks [0];
				for (int ikb = 1; ikb < rgKeyBlocks.Count; ikb++)
				{
					KeyBlock kb = (KeyBlock) rgKeyBlocks [ikb];
					if ((kbCurr.Size + kb.Size) * 2 >=  KeyBlock.TotalLength (kbCurr, kb))
					{
						// merge blocks
						kbCurr.nLast = kb.nLast;
						kbCurr.Size += kb.Size;
					}
					else
					{
						// start a new block
						rgKeyBlocksNew.Add (kbCurr);
						kbCurr = kb;
					}
				}
				rgKeyBlocksNew.Add (kbCurr);
				if (rgKeyBlocks.Count == rgKeyBlocksNew.Count)
					break;
				rgKeyBlocks = rgKeyBlocksNew;
			}

			// initialize the key lists
			foreach (KeyBlock kb in rgKeyBlocks)
				kb.rgKeys = new ArrayList ();

			// fill the key lists
			int iBlockCurr = 0;
			if (rgKeyBlocks.Count > 0) {
				kbCurr = (KeyBlock) rgKeyBlocks [0];
				foreach (object key in rgKeys)
				{
					bool fNextBlock = (key is UInt64) ? (ulong) key > (ulong) kbCurr.nLast :
						System.Convert.ToInt64 (key) > kbCurr.nLast;
					if (fNextBlock)
						kbCurr = (KeyBlock) rgKeyBlocks [++iBlockCurr];
					kbCurr.rgKeys.Add (key);
				}
			}

			// sort the blocks so we can tackle the largest ones first
			rgKeyBlocks.Sort ();

			// okay now we can start...
			ILGenerator ig = ec.ig;
			Label lblEnd = ig.DefineLabel ();	// at the end ;-)
			Label lblDefault = ig.DefineLabel ();

			Type typeKeys = null;
			if (rgKeys.Length > 0)
				typeKeys = rgKeys [0].GetType ();	// used for conversions

			Type compare_type;
			
			if (TypeManager.IsEnumType (SwitchType))
				compare_type = TypeManager.EnumToUnderlying (SwitchType);
			else
				compare_type = SwitchType;
			
			for (int iBlock = rgKeyBlocks.Count - 1; iBlock >= 0; --iBlock)
			{
				KeyBlock kb = ((KeyBlock) rgKeyBlocks [iBlock]);
				lblDefault = (iBlock == 0) ? DefaultTarget : ig.DefineLabel ();
				if (kb.Length <= 2)
				{
					foreach (object key in kb.rgKeys)
					{
						ig.Emit (OpCodes.Ldloc, val);
						EmitObjectInteger (ig, key);
						SwitchLabel sl = (SwitchLabel) Elements [key];
						ig.Emit (OpCodes.Beq, sl.GetILLabel (ec));
					}
				}
				else
				{
					// TODO: if all the keys in the block are the same and there are
					//       no gaps/defaults then just use a range-check.
					if (compare_type == TypeManager.int64_type ||
						compare_type == TypeManager.uint64_type)
					{
						// TODO: optimize constant/I4 cases

						// check block range (could be > 2^31)
						ig.Emit (OpCodes.Ldloc, val);
						EmitObjectInteger (ig, System.Convert.ChangeType (kb.nFirst, typeKeys));
						ig.Emit (OpCodes.Blt, lblDefault);
						ig.Emit (OpCodes.Ldloc, val);
						EmitObjectInteger (ig, System.Convert.ChangeType (kb.nLast, typeKeys));
						ig.Emit (OpCodes.Bgt, lblDefault);

						// normalize range
						ig.Emit (OpCodes.Ldloc, val);
						if (kb.nFirst != 0)
						{
							EmitObjectInteger (ig, System.Convert.ChangeType (kb.nFirst, typeKeys));
							ig.Emit (OpCodes.Sub);
						}
						ig.Emit (OpCodes.Conv_I4);	// assumes < 2^31 labels!
					}
					else
					{
						// normalize range
						ig.Emit (OpCodes.Ldloc, val);
						int nFirst = (int) kb.nFirst;
						if (nFirst > 0)
						{
							IntConstant.EmitInt (ig, nFirst);
							ig.Emit (OpCodes.Sub);
						}
						else if (nFirst < 0)
						{
							IntConstant.EmitInt (ig, -nFirst);
							ig.Emit (OpCodes.Add);
						}
					}

					// first, build the list of labels for the switch
					int iKey = 0;
					int cJumps = kb.Length;
					Label [] rgLabels = new Label [cJumps];
					for (int iJump = 0; iJump < cJumps; iJump++)
					{
						object key = kb.rgKeys [iKey];
						if (System.Convert.ToInt64 (key) == kb.nFirst + iJump)
						{
							SwitchLabel sl = (SwitchLabel) Elements [key];
							rgLabels [iJump] = sl.GetILLabel (ec);
							iKey++;
						}
						else
							rgLabels [iJump] = lblDefault;
					}
					// emit the switch opcode
					ig.Emit (OpCodes.Switch, rgLabels);
				}

				// mark the default for this block
				if (iBlock != 0)
					ig.MarkLabel (lblDefault);
			}

			// TODO: find the default case and emit it here,
			//       to prevent having to do the following jump.
			//       make sure to mark other labels in the default section

			// the last default just goes to the end
			ig.Emit (OpCodes.Br, lblDefault);

			// now emit the code for the sections
			bool fFoundDefault = false;
			foreach (SwitchSection ss in Sections)
			{
				foreach (SwitchLabel sl in ss.Labels)
				{
					ig.MarkLabel (sl.GetILLabel (ec));
					ig.MarkLabel (sl.GetILLabelCode (ec));
					if (sl.Label == null)
					{
						ig.MarkLabel (lblDefault);
						fFoundDefault = true;
					}
				}
				ss.Block.Emit (ec);
				//ig.Emit (OpCodes.Br, lblEnd);
			}
			
			if (!fFoundDefault) {
				ig.MarkLabel (lblDefault);
			}
			ig.MarkLabel (lblEnd);
		}
		//
		// This simple emit switch works, but does not take advantage of the
		// `switch' opcode. 
		// TODO: remove non-string logic from here
		// TODO: binary search strings?
		//
		void SimpleSwitchEmit (EmitContext ec, LocalBuilder val)
		{
			ILGenerator ig = ec.ig;
			Label end_of_switch = ig.DefineLabel ();
			Label next_test = ig.DefineLabel ();
			Label null_target = ig.DefineLabel ();
			bool default_found = false;
			bool first_test = true;
			bool pending_goto_end = false;
			bool null_found;
			bool default_at_end = false;
			
			ig.Emit (OpCodes.Ldloc, val);
			
			if (Elements.Contains (NullLiteral.Null)){
				ig.Emit (OpCodes.Brfalse, null_target);
			} else
				ig.Emit (OpCodes.Brfalse, default_target);
			
			ig.Emit (OpCodes.Ldloc, val);
			ig.Emit (OpCodes.Call, TypeManager.string_isinterneted_string);
			ig.Emit (OpCodes.Stloc, val);
		
			int section_count = Sections.Count;
			for (int section = 0; section < section_count; section++){
				SwitchSection ss = (SwitchSection) Sections [section];
				Label sec_begin = ig.DefineLabel ();

				if (pending_goto_end)
					ig.Emit (OpCodes.Br, end_of_switch);

				int label_count = ss.Labels.Count;
				bool mark_default = false;
				null_found = false;
				for (int label = 0; label < label_count; label++){
					SwitchLabel sl = (SwitchLabel) ss.Labels [label];
					ig.MarkLabel (sl.GetILLabel (ec));
					
					if (!first_test){
						ig.MarkLabel (next_test);
						next_test = ig.DefineLabel ();
					}
					//
					// If we are the default target
					//
					if (sl.Label == null){
						if (label+1 == label_count)
							default_at_end = true;
						mark_default = true;
						default_found = true;
					} else {
						object lit = sl.Converted;

						if (lit is NullLiteral){
							null_found = true;
							if (label_count == 1)
								ig.Emit (OpCodes.Br, next_test);
							continue;
									      
						}
						StringConstant str = (StringConstant) lit;
						
						ig.Emit (OpCodes.Ldloc, val);
						ig.Emit (OpCodes.Ldstr, str.Value);
						if (label_count == 1)
							ig.Emit (OpCodes.Bne_Un, next_test);
						else {
							if (label+1 == label_count)
								ig.Emit (OpCodes.Bne_Un, next_test);
							else
								ig.Emit (OpCodes.Beq, sec_begin);
						}
					}
				}
				if (null_found)
					ig.MarkLabel (null_target);
				ig.MarkLabel (sec_begin);
				foreach (SwitchLabel sl in ss.Labels)
					ig.MarkLabel (sl.GetILLabelCode (ec));

				if (mark_default)
					ig.MarkLabel (default_target);
				ss.Block.Emit (ec);
				pending_goto_end = !ss.Block.HasRet;
				first_test = false;
			}
			ig.MarkLabel (next_test);
			if (default_found){
				if (!default_at_end)
					ig.Emit (OpCodes.Br, default_target);
			} else 
				ig.MarkLabel (default_target);
			ig.MarkLabel (end_of_switch);
		}

		SwitchSection FindSection (SwitchLabel label)
		{
			foreach (SwitchSection ss in Sections){
				foreach (SwitchLabel sl in ss.Labels){
					if (label == sl)
						return ss;
				}
			}

			return null;
		}

		bool ResolveConstantSwitch (EmitContext ec)
		{
			object key = ((Constant) new_expr).GetValue ();
			SwitchLabel label = (SwitchLabel) Elements [key];

			if (label == null)
				return true;

			constant_section = FindSection (label);
			if (constant_section == null)
				return true;

			if (constant_section.Block.Resolve (ec) != true)
				return false;

			return true;
		}

		public override bool Resolve (EmitContext ec)
		{
			Expr = Expr.Resolve (ec);
			if (Expr == null)
				return false;

			new_expr = SwitchGoverningType (ec, Expr.Type);
			if (new_expr == null){
				Report.Error (151, loc, "An integer type or string was expected for switch");
				return false;
			}

			// Validate switch.
			SwitchType = new_expr.Type;

			if (!CheckSwitch (ec))
				return false;

			Switch old_switch = ec.Switch;
			ec.Switch = this;
			ec.Switch.SwitchType = SwitchType;

			Report.Debug (1, "START OF SWITCH BLOCK", loc, ec.CurrentBranching);
			ec.StartFlowBranching (FlowBranching.BranchingType.Switch, loc);

			is_constant = new_expr is Constant;
			if (is_constant) {
				object key = ((Constant) new_expr).GetValue ();
				SwitchLabel label = (SwitchLabel) Elements [key];

				constant_section = FindSection (label);
			}

			bool first = true;
			foreach (SwitchSection ss in Sections){
				if (!first)
					ec.CurrentBranching.CreateSibling (
						null, FlowBranching.SiblingType.SwitchSection);
				else
					first = false;

				if (is_constant && (ss != constant_section)) {
					// If we're a constant switch, we're only emitting
					// one single section - mark all the others as
					// unreachable.
					ec.CurrentBranching.CurrentUsageVector.Goto ();
					if (!ss.Block.ResolveUnreachable (ec, true))
						return false;
				} else {
					if (!ss.Block.Resolve (ec))
					return false;
			}
			}

			if (!got_default)
				ec.CurrentBranching.CreateSibling (
					null, FlowBranching.SiblingType.SwitchSection);

			FlowBranching.Reachability reachability = ec.EndFlowBranching ();
			ec.Switch = old_switch;

			Report.Debug (1, "END OF SWITCH BLOCK", loc, ec.CurrentBranching,
				      reachability);

			return true;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			// Store variable for comparission purposes
			LocalBuilder value;
			if (!is_constant) {
				value = ig.DeclareLocal (SwitchType);
			new_expr.Emit (ec);
				ig.Emit (OpCodes.Stloc, value);
			} else
				value = null;

			default_target = ig.DefineLabel ();

			//
			// Setup the codegen context
			//
			Label old_end = ec.LoopEnd;
			Switch old_switch = ec.Switch;
			
			ec.LoopEnd = ig.DefineLabel ();
			ec.Switch = this;

			// Emit Code.
			if (is_constant) {
				if (constant_section != null)
					constant_section.Block.Emit (ec);
			} else if (SwitchType == TypeManager.string_type)
				SimpleSwitchEmit (ec, value);
			else
				TableSwitchEmit (ec, value);

			// Restore context state. 
			ig.MarkLabel (ec.LoopEnd);

			//
			// Restore the previous context
			//
			ec.LoopEnd = old_end;
			ec.Switch = old_switch;
		}
	}

	public abstract class ExceptionStatement : Statement
	{
		public abstract void EmitFinally (EmitContext ec);

		protected bool emit_finally = true;
		ArrayList parent_vectors;

		protected void DoEmitFinally (EmitContext ec)
		{
			if (emit_finally)
				ec.ig.BeginFinallyBlock ();
			else
				ec.CurrentIterator.MarkFinally (ec, parent_vectors);
			EmitFinally (ec);
		}

		protected void ResolveFinally (FlowBranchingException branching)
		{
			emit_finally = branching.EmitFinally;
			if (!emit_finally)
				branching.Parent.StealFinallyClauses (ref parent_vectors);
		}
	}

	public class Lock : ExceptionStatement {
		Expression expr;
		Statement Statement;
		LocalBuilder temp;
			
		public Lock (Expression expr, Statement stmt, Location l)
		{
			this.expr = expr;
			Statement = stmt;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			if (expr.Type.IsValueType){
				Error (185, "lock statement requires the expression to be " +
				       " a reference type (type is: `{0}'",
				       TypeManager.CSharpName (expr.Type));
				return false;
			}

			FlowBranchingException branching = ec.StartFlowBranching (this);
			bool ok = Statement.Resolve (ec);
			if (!ok) {
				ec.KillFlowBranching ();
				return false;
			}

			ResolveFinally (branching);

			FlowBranching.Reachability reachability = ec.EndFlowBranching ();
			if (reachability.Returns != FlowBranching.FlowReturns.Always) {
				// Unfortunately, System.Reflection.Emit automatically emits
				// a leave to the end of the finally block.
				// This is a problem if `returns' is true since we may jump
				// to a point after the end of the method.
				// As a workaround, emit an explicit ret here.
				ec.NeedReturnLabel ();
			}

			return true;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			Type type = expr.Type;
			
			ILGenerator ig = ec.ig;
			temp = ig.DeclareLocal (type);
				
			expr.Emit (ec);
			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Stloc, temp);
			ig.Emit (OpCodes.Call, TypeManager.void_monitor_enter_object);

			// try
			if (emit_finally)
				ig.BeginExceptionBlock ();
			Statement.Emit (ec);
			
			// finally
			DoEmitFinally (ec);
			if (emit_finally)
				ig.EndExceptionBlock ();
		}

		public override void EmitFinally (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			ig.Emit (OpCodes.Ldloc, temp);
			ig.Emit (OpCodes.Call, TypeManager.void_monitor_exit_object);
		}
	}

	public class Unchecked : Statement {
		public readonly Block Block;
		
		public Unchecked (Block b)
		{
			Block = b;
			b.Unchecked = true;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool previous_state_const = ec.ConstantCheckState;

			ec.CheckState = false;
			ec.ConstantCheckState = false;
			bool ret = Block.Resolve (ec);
			ec.CheckState = previous_state;
			ec.ConstantCheckState = previous_state_const;

			return ret;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool previous_state_const = ec.ConstantCheckState;
			
			ec.CheckState = false;
			ec.ConstantCheckState = false;
			Block.Emit (ec);
			ec.CheckState = previous_state;
			ec.ConstantCheckState = previous_state_const;
		}
	}

	public class Checked : Statement {
		public readonly Block Block;
		
		public Checked (Block b)
		{
			Block = b;
			b.Unchecked = false;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool previous_state_const = ec.ConstantCheckState;
			
			ec.CheckState = true;
			ec.ConstantCheckState = true;
			bool ret = Block.Resolve (ec);
			ec.CheckState = previous_state;
			ec.ConstantCheckState = previous_state_const;

			return ret;
		}

		protected override void DoEmit (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool previous_state_const = ec.ConstantCheckState;
			
			ec.CheckState = true;
			ec.ConstantCheckState = true;
			Block.Emit (ec);
			ec.CheckState = previous_state;
			ec.ConstantCheckState = previous_state_const;
		}
	}

	public class Unsafe : Statement {
		public readonly Block Block;

		public Unsafe (Block b)
		{
			Block = b;
			Block.Unsafe = true;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool previous_state = ec.InUnsafe;
			bool val;
			
			ec.InUnsafe = true;
			val = Block.Resolve (ec);
			ec.InUnsafe = previous_state;

			return val;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			bool previous_state = ec.InUnsafe;
			
			ec.InUnsafe = true;
			Block.Emit (ec);
			ec.InUnsafe = previous_state;
		}
	}

	// 
	// Fixed statement
	//
	public class Fixed : Statement {
		Expression type;
		ArrayList declarators;
		Statement statement;
		Type expr_type;
		FixedData[] data;
		bool has_ret;

		struct FixedData {
			public bool is_object;
			public LocalInfo vi;
			public Expression expr;
			public Expression converted;
		}			

		public Fixed (Expression type, ArrayList decls, Statement stmt, Location l)
		{
			this.type = type;
			declarators = decls;
			statement = stmt;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!ec.InUnsafe){
				Expression.UnsafeError (loc);
				return false;
			}
			
			TypeExpr texpr = type.ResolveAsTypeTerminal (ec);
			if (texpr == null)
				return false;

			expr_type = texpr.Type;

			CheckObsolete (expr_type);

			if (ec.RemapToProxy){
				Report.Error (-210, loc, "Fixed statement not allowed in iterators");
				return false;
			}
			
			data = new FixedData [declarators.Count];

			if (!expr_type.IsPointer){
				Report.Error (209, loc, "Variables in a fixed statement must be pointers");
				return false;
			}
			
			int i = 0;
			foreach (Pair p in declarators){
				LocalInfo vi = (LocalInfo) p.First;
				Expression e = (Expression) p.Second;

				vi.VariableInfo.SetAssigned (ec);
				vi.ReadOnly = true;

				//
				// The rules for the possible declarators are pretty wise,
				// but the production on the grammar is more concise.
				//
				// So we have to enforce these rules here.
				//
				// We do not resolve before doing the case 1 test,
				// because the grammar is explicit in that the token &
				// is present, so we need to test for this particular case.
				//

				if (e is Cast){
					Report.Error (254, loc, "Cast expression not allowed as right hand expression in fixed statement");
					return false;
				}
				
				//
				// Case 1: & object.
				//
				if (e is Unary && ((Unary) e).Oper == Unary.Operator.AddressOf){
					Expression child = ((Unary) e).Expr;

					if (child is ParameterReference || child is LocalVariableReference){
						Report.Error (
							213, loc, 
							"No need to use fixed statement for parameters or " +
							"local variable declarations (address is already " +
							"fixed)");
						return false;
					}

					ec.InFixedInitializer = true;
					e = e.Resolve (ec);
					ec.InFixedInitializer = false;
					if (e == null)
						return false;

					child = ((Unary) e).Expr;
					
					if (!TypeManager.VerifyUnManaged (child.Type, loc))
						return false;

					data [i].is_object = true;
					data [i].expr = e;
					data [i].converted = null;
					data [i].vi = vi;
					i++;

					continue;
				}

				ec.InFixedInitializer = true;
				e = e.Resolve (ec);
				ec.InFixedInitializer = false;
				if (e == null)
					return false;

				//
				// Case 2: Array
				//
				if (e.Type.IsArray){
					Type array_type = TypeManager.GetElementType (e.Type);
					
					//
					// Provided that array_type is unmanaged,
					//
					if (!TypeManager.VerifyUnManaged (array_type, loc))
						return false;

					//
					// and T* is implicitly convertible to the
					// pointer type given in the fixed statement.
					//
					ArrayPtr array_ptr = new ArrayPtr (e, loc);
					
					Expression converted = Convert.WideningConversionRequired (
						ec, array_ptr, vi.VariableType, loc);
					if (converted == null)
						return false;

					data [i].is_object = false;
					data [i].expr = e;
					data [i].converted = converted;
					data [i].vi = vi;
					i++;

					continue;
				}

				//
				// Case 3: string
				//
				if (e.Type == TypeManager.string_type){
					data [i].is_object = false;
					data [i].expr = e;
					data [i].converted = null;
					data [i].vi = vi;
					i++;
					continue;
				}

				//
				// For other cases, flag a `this is already fixed expression'
				//
				if (e is LocalVariableReference || e is ParameterReference ||
				    Convert.WideningConversionExists (ec, e, vi.VariableType)){
				    
					Report.Error (245, loc, "right hand expression is already fixed, no need to use fixed statement ");
					return false;
				}

				Report.Error (245, loc, "Fixed statement only allowed on strings, arrays or address-of expressions");
				return false;
			}

			ec.StartFlowBranching (FlowBranching.BranchingType.Conditional, loc);

			if (!statement.Resolve (ec)) {
				ec.KillFlowBranching ();
				return false;
			}

			FlowBranching.Reachability reachability = ec.EndFlowBranching ();
			has_ret = reachability.IsUnreachable;

			return true;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			LocalBuilder [] clear_list = new LocalBuilder [data.Length];
			
			for (int i = 0; i < data.Length; i++) {
				LocalInfo vi = data [i].vi;

				//
				// Case 1: & object.
				//
				if (data [i].is_object) {
					//
					// Store pointer in pinned location
					//
					data [i].expr.Emit (ec);
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);
					clear_list [i] = vi.LocalBuilder;
					continue;
				}

				//
				// Case 2: Array
				//
				if (data [i].expr.Type.IsArray){
					//
					// Store pointer in pinned location
					//
					data [i].converted.Emit (ec);
					
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);
					clear_list [i] = vi.LocalBuilder;
					continue;
				}

				//
				// Case 3: string
				//
				if (data [i].expr.Type == TypeManager.string_type){
					LocalBuilder pinned_string = TypeManager.DeclareLocalPinned (ig, TypeManager.string_type);
					clear_list [i] = pinned_string;
					
					data [i].expr.Emit (ec);
					ig.Emit (OpCodes.Stloc, pinned_string);

					Expression sptr = new StringPtr (pinned_string, loc);
					Expression converted = Convert.WideningConversionRequired (
						ec, sptr, vi.VariableType, loc);
					
					if (converted == null)
						continue;

					converted.Emit (ec);
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);
				}
			}

			statement.Emit (ec);

			if (has_ret)
				return;

			//
			// Clear the pinned variable
			//
			for (int i = 0; i < data.Length; i++) {
				if (data [i].is_object || data [i].expr.Type.IsArray) {
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Conv_U);
					ig.Emit (OpCodes.Stloc, clear_list [i]);
				} else if (data [i].expr.Type == TypeManager.string_type){
					ig.Emit (OpCodes.Ldnull);
					ig.Emit (OpCodes.Stloc, clear_list [i]);
				}
			}
		}
	}
	
	public class Catch: Statement {
		public readonly string Name;
		public readonly Block  Block;

		Expression type_expr;
		Type type;
		
		public Catch (Expression type, string name, Block block, Location l)
		{
			type_expr = type;
			Name = name;
			Block = block;
			loc = l;
		}

		public Type CatchType {
			get {
				return type;
			}
		}

		public bool IsGeneral {
			get {
				return type_expr == null;
			}
		}

		protected override void DoEmit(EmitContext ec)
		{
		}

		public override bool Resolve (EmitContext ec)
		{
			if (type_expr != null) {
				TypeExpr te = type_expr.ResolveAsTypeTerminal (ec);
				if (te == null)
					return false;

				type = te.Type;

				CheckObsolete (type);

				if (type != TypeManager.exception_type && !type.IsSubclassOf (TypeManager.exception_type)){
					Error (155, "The type caught or thrown must be derived from System.Exception");
					return false;
				}
			} else
				type = null;

			return Block.Resolve (ec);
		}
	}

	public class Try : ExceptionStatement {
		public readonly Block Fini, Block;
		public readonly ArrayList Specific;
		public readonly Catch General;

		bool need_exc_block;
		
		//
		// specific, general and fini might all be null.
		//
		public Try (Block block, ArrayList specific, Catch general, Block fini, Location l)
		{
			if (specific == null && general == null){
				Console.WriteLine ("CIR.Try: Either specific or general have to be non-null");
			}
			
			this.Block = block;
			this.Specific = specific;
			this.General = general;
			this.Fini = fini;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;
			
			FlowBranchingException branching = ec.StartFlowBranching (this);

			Report.Debug (1, "START OF TRY BLOCK", Block.StartLocation);

			if (!Block.Resolve (ec))
				ok = false;

			FlowBranching.UsageVector vector = ec.CurrentBranching.CurrentUsageVector;

			Report.Debug (1, "START OF CATCH BLOCKS", vector);

			Type[] prevCatches = new Type [Specific.Count];
			int last_index = 0;
			foreach (Catch c in Specific){
				ec.CurrentBranching.CreateSibling (
					c.Block, FlowBranching.SiblingType.Catch);

				Report.Debug (1, "STARTED SIBLING FOR CATCH", ec.CurrentBranching);

				if (c.Name != null) {
					LocalInfo vi = c.Block.GetLocalInfo (c.Name);
					if (vi == null)
						throw new Exception ();

					vi.VariableInfo = null;
				}

				if (!c.Resolve (ec))
					return false;

				Type resolvedType = c.CatchType;
				for (int ii = 0; ii < last_index; ++ii) {
					if (resolvedType == prevCatches [ii] || resolvedType.IsSubclassOf (prevCatches [ii])) {
						Report.Error (160, c.loc, "A previous catch clause already catches all exceptions of this or a super type '{0}'", prevCatches [ii].FullName);
						return false;
					}
				}

				prevCatches [last_index++] = resolvedType;
				need_exc_block = true;
			}

			Report.Debug (1, "END OF CATCH BLOCKS", ec.CurrentBranching);

			if (General != null){
				ec.CurrentBranching.CreateSibling (
					General.Block, FlowBranching.SiblingType.Catch);

				Report.Debug (1, "STARTED SIBLING FOR GENERAL", ec.CurrentBranching);

				if (!General.Resolve (ec))
					ok = false;

				need_exc_block = true;
			}

			Report.Debug (1, "END OF GENERAL CATCH BLOCKS", ec.CurrentBranching);

			if (Fini != null) {
				if (ok)
					ec.CurrentBranching.CreateSibling (
						Fini, FlowBranching.SiblingType.Finally);

				Report.Debug (1, "STARTED SIBLING FOR FINALLY", ec.CurrentBranching, vector);

				if (!Fini.Resolve (ec))
					ok = false;
			}

			ResolveFinally (branching);
			need_exc_block |= emit_finally;

			FlowBranching.Reachability reachability = ec.EndFlowBranching ();

			FlowBranching.UsageVector f_vector = ec.CurrentBranching.CurrentUsageVector;

			Report.Debug (1, "END OF TRY", ec.CurrentBranching, reachability, vector, f_vector);

			if (reachability.Returns != FlowBranching.FlowReturns.Always) {
				// Unfortunately, System.Reflection.Emit automatically emits
				// a leave to the end of the finally block.  This is a problem
				// if `returns' is true since we may jump to a point after the
				// end of the method.
				// As a workaround, emit an explicit ret here.
				ec.NeedReturnLabel ();
			}

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (need_exc_block)
				ig.BeginExceptionBlock ();
			Block.Emit (ec);

			foreach (Catch c in Specific){
				LocalInfo vi;
				
				ig.BeginCatchBlock (c.CatchType);

				if (c.Name != null){
					vi = c.Block.GetLocalInfo (c.Name);
					if (vi == null)
						throw new Exception ("Variable does not exist in this block");

					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);
				} else
					ig.Emit (OpCodes.Pop);
				
				c.Block.Emit (ec);
			}

			if (General != null){
				ig.BeginCatchBlock (TypeManager.object_type);
				ig.Emit (OpCodes.Pop);
				General.Block.Emit (ec);
			}

			DoEmitFinally (ec);
			if (need_exc_block)
				ig.EndExceptionBlock ();
		}

		public override void EmitFinally (EmitContext ec)
		{
			if (Fini != null){
				Fini.Emit (ec);
			}
		}
	}

	public class Using : ExceptionStatement {
		object expression_or_block;
		Statement Statement;
		ArrayList var_list;
		Expression expr;
		Type expr_type;
		Expression conv;
		Expression [] resolved_vars;
		Expression [] converted_vars;
		ExpressionStatement [] assign;
		LocalBuilder local_copy;
		
		public Using (object expression_or_block, Statement stmt, Location l)
		{
			this.expression_or_block = expression_or_block;
			Statement = stmt;
			loc = l;
		}

		//
		// Resolves for the case of using using a local variable declaration.
		//
		bool ResolveLocalVariableDecls (EmitContext ec)
		{
			int i = 0;

			TypeExpr texpr = expr.ResolveAsTypeTerminal (ec);
			if (texpr == null)
				return false;

			expr_type = texpr.Type;

			//
			// The type must be an IDisposable or an implicit conversion
			// must exist.
			//
			converted_vars = new Expression [var_list.Count];
			resolved_vars = new Expression [var_list.Count];
			assign = new ExpressionStatement [var_list.Count];

			bool need_conv = !TypeManager.ImplementsInterface (
				expr_type, TypeManager.idisposable_type);

			foreach (DictionaryEntry e in var_list){
				Expression var = (Expression) e.Key;

				var = var.ResolveLValue (ec, new EmptyExpression ());
				if (var == null)
					return false;

				resolved_vars [i] = var;

				if (!need_conv) {
					i++;
					continue;
				}

				converted_vars [i] = Convert.WideningConversionRequired (
					ec, var, TypeManager.idisposable_type, loc);

				if (converted_vars [i] == null)
					return false;

				i++;
			}

			i = 0;
			foreach (DictionaryEntry e in var_list){
				Expression var = resolved_vars [i];
				Expression new_expr = (Expression) e.Value;
				Expression a;

				a = new Assign (var, new_expr, loc);
				a = a.Resolve (ec);
				if (a == null)
					return false;

				if (!need_conv)
					converted_vars [i] = var;
				assign [i] = (ExpressionStatement) a;
				i++;
			}

			return true;
		}

		bool ResolveExpression (EmitContext ec)
		{
			if (!TypeManager.ImplementsInterface (expr_type, TypeManager.idisposable_type)){
				conv = Convert.WideningConversionRequired (
					ec, expr, TypeManager.idisposable_type, loc);

				if (conv == null)
					return false;
			}

			return true;
		}
		
		//
		// Emits the code for the case of using using a local variable declaration.
		//
		void EmitLocalVariableDecls (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			int i = 0;

			for (i = 0; i < assign.Length; i++) {
				assign [i].EmitStatement (ec);

				if (emit_finally)
					ig.BeginExceptionBlock ();
			}
			Statement.Emit (ec);

			var_list.Reverse ();

			DoEmitFinally (ec);
		}

		void EmitLocalVariableDeclFinally (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			int i = assign.Length;
			for (int ii = 0; ii < var_list.Count; ++ii){
				Expression var = resolved_vars [--i];
				Label skip = ig.DefineLabel ();
				
				ig.BeginFinallyBlock ();
				
				if (!var.Type.IsValueType) {
					var.Emit (ec);
					ig.Emit (OpCodes.Brfalse, skip);
					converted_vars [i].Emit (ec);
					ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				} else {
					Expression ml = Expression.MemberLookup(ec, TypeManager.idisposable_type, var.Type, "Dispose", Mono.CSharp.Location.Null);

					if (!(ml is MethodGroupExpr)) {
						var.Emit (ec);
						ig.Emit (OpCodes.Box, var.Type);
						ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
					} else {
						MethodInfo mi = null;

						foreach (MethodInfo mk in ((MethodGroupExpr) ml).Methods) {
							if (TypeManager.GetArgumentTypes (mk).Length == 0) {
								mi = mk;
								break;
							}
						}

						if (mi == null) {
							Report.Error(-100, Mono.CSharp.Location.Null, "Internal error: No Dispose method which takes 0 parameters.");
							return;
						}

						IMemoryLocation mloc = (IMemoryLocation) var;

						mloc.AddressOf (ec, AddressOp.Load);
						ig.Emit (OpCodes.Call, mi);
					}
				}

				ig.MarkLabel (skip);

				if (emit_finally) {
					ig.EndExceptionBlock ();
					if (i > 0)
						ig.BeginFinallyBlock ();
				}
			}
		}

		void EmitExpression (EmitContext ec)
		{
			//
			// Make a copy of the expression and operate on that.
			//
			ILGenerator ig = ec.ig;
			local_copy = ig.DeclareLocal (expr_type);
			if (conv != null)
				conv.Emit (ec);
			else
				expr.Emit (ec);
			ig.Emit (OpCodes.Stloc, local_copy);

			if (emit_finally)
				ig.BeginExceptionBlock ();

			Statement.Emit (ec);
			
			DoEmitFinally (ec);
			if (emit_finally)
				ig.EndExceptionBlock ();
		}

		void EmitExpressionFinally (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			if (!local_copy.LocalType.IsValueType) {
				Label skip = ig.DefineLabel ();
				ig.Emit (OpCodes.Ldloc, local_copy);
				ig.Emit (OpCodes.Brfalse, skip);
				ig.Emit (OpCodes.Ldloc, local_copy);
				ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				ig.MarkLabel (skip);
			} else {
				Expression ml = Expression.MemberLookup(ec, TypeManager.idisposable_type, local_copy.LocalType, "Dispose", Mono.CSharp.Location.Null);

				if (!(ml is MethodGroupExpr)) {
					ig.Emit (OpCodes.Ldloc, local_copy);
					ig.Emit (OpCodes.Box, local_copy.LocalType);
					ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				} else {
					MethodInfo mi = null;

					foreach (MethodInfo mk in ((MethodGroupExpr) ml).Methods) {
						if (TypeManager.GetArgumentTypes (mk).Length == 0) {
							mi = mk;
							break;
						}
					}

					if (mi == null) {
						Report.Error(-100, Mono.CSharp.Location.Null, "Internal error: No Dispose method which takes 0 parameters.");
						return;
					}

					ig.Emit (OpCodes.Ldloca, local_copy);
					ig.Emit (OpCodes.Call, mi);
				}
			}
		}
		
		public override bool Resolve (EmitContext ec)
		{
			if (expression_or_block is DictionaryEntry){
				expr = (Expression) ((DictionaryEntry) expression_or_block).Key;
				var_list = (ArrayList)((DictionaryEntry)expression_or_block).Value;

				if (!ResolveLocalVariableDecls (ec))
					return false;

			} else if (expression_or_block is Expression){
				expr = (Expression) expression_or_block;

				expr = expr.Resolve (ec);
				if (expr == null)
					return false;

				expr_type = expr.Type;

				if (!ResolveExpression (ec))
					return false;
			}

			FlowBranchingException branching = ec.StartFlowBranching (this);

			bool ok = Statement.Resolve (ec);

			if (!ok) {
				ec.KillFlowBranching ();
				return false;
			}

			ResolveFinally (branching);					
			FlowBranching.Reachability reachability = ec.EndFlowBranching ();

			if (reachability.Returns != FlowBranching.FlowReturns.Always) {
				// Unfortunately, System.Reflection.Emit automatically emits a leave
				// to the end of the finally block.  This is a problem if `returns'
				// is true since we may jump to a point after the end of the method.
				// As a workaround, emit an explicit ret here.
				ec.NeedReturnLabel ();
			}

			return true;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			if (expression_or_block is DictionaryEntry)
				EmitLocalVariableDecls (ec);
			else if (expression_or_block is Expression)
				EmitExpression (ec);
		}

		public override void EmitFinally (EmitContext ec)
		{
			if (expression_or_block is DictionaryEntry)
				EmitLocalVariableDeclFinally (ec);
			else if (expression_or_block is Expression)
				EmitExpressionFinally (ec);
		}
	}

	public class WithTarget {
		Expression original_expr;
		Expression resolved_expr;
		Type target_type;
		LocalBuilder temp_builder;
		bool stores_address;
		bool is_resolved;
		bool resolve_failed;

		public readonly Location Location;

		public WithTarget (Expression expr, Location l)
		{
			original_expr = expr;
			Location = l;
		}

		public Type TargetType {
			get {
				return target_type;
			}
		}

		public bool StoresAddress {
			get {
				return stores_address;
			}
		}

		public bool Resolve (EmitContext ec)
		{
			if (is_resolved)
				return !resolve_failed;

			is_resolved = true;

			resolved_expr = original_expr.Resolve (ec, ResolveFlags.VariableOrValue);
			if (resolved_expr == null) {
				resolve_failed = true;
				return false;
			}

			switch (resolved_expr.eclass) {
			case ExprClass.Value:
			case ExprClass.Variable:
			case ExprClass.PropertyAccess:
			case ExprClass.IndexerAccess:
				break;
			default:
				resolved_expr.Error_UnexpectedKind ("value", Location);
				resolve_failed = true;
				return false;
			}

			target_type = resolved_expr.Type;
			stores_address = target_type.IsValueType && (resolved_expr is IMemoryLocation);
			return true;
		}

		public void EmitStore (EmitContext ec)
		{
			if (temp_builder != null)
				throw new Exception ("With target already emitted");

			Type temp_type = stores_address ?
				TypeManager.GetReferenceType (target_type) :
				target_type;
			temp_builder = ec.GetTemporaryLocal (temp_type);

			if (stores_address)
				((IMemoryLocation) resolved_expr).AddressOf (ec, AddressOp.Load);
			else
				resolved_expr.Emit (ec);

			ec.ig.Emit (OpCodes.Stloc, temp_builder);
		}

		public void Emit (EmitContext ec)
		{
			if (temp_builder == null)
				throw new Exception ("With target has not been emitted");

			ec.ig.Emit (OpCodes.Ldloc, temp_builder);
			if (stores_address)
				Expression.LoadFromPtr (ec.ig, target_type);
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			if (!stores_address)
				throw new Exception ("With target does not store an address");

			if (temp_builder == null)
				throw new Exception ("With target has not been emitted");

			ec.ig.Emit (OpCodes.Ldloc, temp_builder);
		}

		public void Release (EmitContext ec)
		{
			if (temp_builder == null)
				return;

			Type temp_type = stores_address ?
				TypeManager.GetReferenceType (target_type) :
				target_type;
			ec.FreeTemporaryLocal (temp_builder, temp_type);
			temp_builder = null;
		}
	}

	public class WithReceiver : Expression {
		WithTarget target;

		public WithReceiver (WithTarget target, Location l)
		{
			this.target = target;
			loc = l;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (!target.Resolve (ec))
				return null;

			Expression receiver;
			if (target.StoresAddress)
				receiver = new WithAddressReceiver (target, loc);
			else
				receiver = new WithValueReceiver (target, loc);

			return receiver.Resolve (ec);
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Should not happen");
		}
	}

	class WithValueReceiver : Expression {
		protected readonly WithTarget target;

		public WithValueReceiver (WithTarget target, Location l)
		{
			this.target = target;
			loc = l;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (!target.Resolve (ec))
				return null;

			type = target.TargetType;
			eclass = target.TargetType.IsValueType ? ExprClass.Value : ExprClass.Variable;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			target.Emit (ec);
		}
	}

	class WithAddressReceiver : WithValueReceiver, IMemoryLocation {
		public WithAddressReceiver (WithTarget target, Location l)
			: base (target, l)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			Expression e = base.DoResolve (ec);
			if (e == null)
				return null;

			eclass = ExprClass.Variable;
			return this;
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			target.AddressOf (ec, mode);
		}
	}

	public class VBWith : Statement {
		WithTarget target;
		Statement statement;

		public VBWith (WithTarget target, Statement statement, Location l)
		{
			this.target = target;
			this.statement = statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!target.Resolve (ec))
				return false;

			return statement.Resolve (ec);
		}

		protected override void DoEmit (EmitContext ec)
		{
			target.EmitStore (ec);
			statement.Emit (ec);
			target.Release (ec);
		}
	}

	/// <summary>
	///   Implementation of the foreach C# statement
	/// </summary>
	public class Foreach : ExceptionStatement {
		Expression type;
		Expression variable;
		Expression expr;
		Statement statement;
		ForeachHelperMethods hm;
		Expression empty, conv;
		Type array_type, element_type;
		Type var_type;
		VariableStorage enumerator;
		
		public Foreach (Expression type, LocalVariableReference var, Expression expr,
				Statement stmt, Location l)
		{
			this.type = type;
			this.variable = var;
			this.expr = expr;
			statement = stmt;
			loc = l;
		}
		
		public override bool Resolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			if (expr is NullLiteral) {
				Report.Error (186, expr.Location, "Use of null is not valid in this context");
				return false;
			}

			TypeExpr texpr = type.ResolveAsTypeTerminal (ec);
			if (texpr == null)
				return false;
			
			var_type = texpr.Type;

			//
			// We need an instance variable.  Not sure this is the best
			// way of doing this.
			//
			// FIXME: When we implement propertyaccess, will those turn
			// out to return values in ExprClass?  I think they should.
			//
			if (!(expr.eclass == ExprClass.Variable || expr.eclass == ExprClass.Value ||
			      expr.eclass == ExprClass.PropertyAccess || expr.eclass == ExprClass.IndexerAccess)){
				error1579 (expr.Type);
				return false;
			}

			if (expr.Type.IsArray) {
				array_type = expr.Type;
				element_type = TypeManager.GetElementType (array_type);

				empty = new EmptyExpression (element_type);
			} else {
				hm = ProbeCollectionType (ec, expr.Type);
				if (hm == null){
					error1579 (expr.Type);
					return false;
				}			

				array_type = expr.Type;
				element_type = hm.element_type;

				empty = new EmptyExpression (hm.element_type);
			}

			bool ok = true;

			ec.StartFlowBranching (VBLoopKind.For, loc);
			ec.CurrentBranching.CreateSibling ();

 			//
			//
			// FIXME: maybe we can apply the same trick we do in the
			// array handling to avoid creating empty and conv in some cases.
			//
			// Although it is not as important in this case, as the type
			// will not likely be object (what the enumerator will return).
			//
			conv = Convert.WideningAndNarrowingConversion (ec, empty, var_type, loc);
			if (conv == null)
				ok = false;

			variable = variable.ResolveLValue (ec, empty);
			if (variable == null)
				ok = false;

			bool disposable = (hm != null) && hm.is_disposable;
			FlowBranchingException branching = null;
			if (disposable)
				branching = ec.StartFlowBranching (this);

			if (!statement.Resolve (ec))
				ok = false;

			if (disposable) {
				ResolveFinally (branching);
				ec.EndFlowBranching ();
			} else
				emit_finally = true;

			ec.EndFlowBranching ();

			return ok;
		}
		
		//
		// Retrieves a `public bool MoveNext ()' method from the Type `t'
		//
		static MethodInfo FetchMethodMoveNext (Type t)
		{
			MemberList move_next_list;
			
			move_next_list = TypeContainer.FindMembers (
				t, MemberTypes.Method,
				BindingFlags.Public | BindingFlags.Instance,
				Type.FilterName, "MoveNext");
			if (move_next_list.Count == 0)
				return null;

			foreach (MemberInfo m in move_next_list){
				MethodInfo mi = (MethodInfo) m;
				Type [] args;
				
				args = TypeManager.GetArgumentTypes (mi);
				if (args != null && args.Length == 0){
					if (TypeManager.TypeToCoreType (mi.ReturnType) == TypeManager.bool_type)
						return mi;
				}
			}
			return null;
		}
		
		//
		// Retrieves a `public T get_Current ()' method from the Type `t'
		//
		static MethodInfo FetchMethodGetCurrent (Type t)
		{
			MemberList get_current_list;

			get_current_list = TypeContainer.FindMembers (
				t, MemberTypes.Method,
				BindingFlags.Public | BindingFlags.Instance,
				Type.FilterName, "get_Current");
			if (get_current_list.Count == 0)
				return null;

			foreach (MemberInfo m in get_current_list){
				MethodInfo mi = (MethodInfo) m;
				Type [] args;

				args = TypeManager.GetArgumentTypes (mi);
				if (args != null && args.Length == 0)
					return mi;
			}
			return null;
		}

		// 
		// Retrieves a `public void Dispose ()' method from the Type `t'
		//
		static MethodInfo FetchMethodDispose (Type t)
		{
			MemberList dispose_list;
			
			dispose_list = TypeContainer.FindMembers (
				t, MemberTypes.Method,
				BindingFlags.Public | BindingFlags.Instance,
				Type.FilterName, "Dispose");
			if (dispose_list.Count == 0)
				return null;

			foreach (MemberInfo m in dispose_list){
				MethodInfo mi = (MethodInfo) m;
				Type [] args;
				
				args = TypeManager.GetArgumentTypes (mi);
				if (args != null && args.Length == 0){
					if (mi.ReturnType == TypeManager.void_type)
						return mi;
				}
			}
			return null;
		}

		// 
		// This struct records the helper methods used by the Foreach construct
		//
		class ForeachHelperMethods {
			public EmitContext ec;
			public MethodInfo get_enumerator;
			public MethodInfo move_next;
			public MethodInfo get_current;
			public Type element_type;
			public Type enumerator_type;
			public bool is_disposable;

			public ForeachHelperMethods (EmitContext ec)
			{
				this.ec = ec;
				this.element_type = TypeManager.object_type;
				this.enumerator_type = TypeManager.ienumerator_type;
				this.is_disposable = true;
			}
		}
		
		static bool GetEnumeratorFilter (MemberInfo m, object criteria)
		{
			if (m == null)
				return false;

			if (!(m is MethodInfo))
				return false;
			
			if (m.Name != "GetEnumerator")
				return false;

			MethodInfo mi = (MethodInfo) m;
			Type [] args = TypeManager.GetArgumentTypes (mi);
			if (args != null){
				if (args.Length != 0)
					return false;
			}
			ForeachHelperMethods hm = (ForeachHelperMethods) criteria;

			// Check whether GetEnumerator is public
			if ((mi.Attributes & MethodAttributes.Public) != MethodAttributes.Public)
					return false;

			if ((mi.ReturnType == TypeManager.ienumerator_type) && (mi.DeclaringType == TypeManager.string_type))
				//
				// Apply the same optimization as MS: skip the GetEnumerator
				// returning an IEnumerator, and use the one returning a 
				// CharEnumerator instead. This allows us to avoid the 
				// try-finally block and the boxing.
				//
				return false;

			//
			// Ok, we can access it, now make sure that we can do something
			// with this `GetEnumerator'
			//

			Type return_type = mi.ReturnType;
			if (mi.ReturnType == TypeManager.ienumerator_type ||
			    TypeManager.ienumerator_type.IsAssignableFrom (return_type) ||
			    (!RootContext.StdLib && TypeManager.ImplementsInterface (return_type, TypeManager.ienumerator_type))) {
				
				//
				// If it is not an interface, lets try to find the methods ourselves.
				// For example, if we have:
				// public class Foo : IEnumerator { public bool MoveNext () {} public int Current { get {}}}
				// We can avoid the iface call. This is a runtime perf boost.
				// even bigger if we have a ValueType, because we avoid the cost
				// of boxing.
				//
				// We have to make sure that both methods exist for us to take
				// this path. If one of the methods does not exist, we will just
				// use the interface. Sadly, this complex if statement is the only
				// way I could do this without a goto
				//
				
				if (return_type.IsInterface ||
				    (hm.move_next = FetchMethodMoveNext (return_type)) == null ||
				    (hm.get_current = FetchMethodGetCurrent (return_type)) == null) {
					
					hm.move_next = TypeManager.bool_movenext_void;
					hm.get_current = TypeManager.object_getcurrent_void;
					return true;    
				}

			} else {

				//
				// Ok, so they dont return an IEnumerable, we will have to
				// find if they support the GetEnumerator pattern.
				//
				
				hm.move_next = FetchMethodMoveNext (return_type);
				if (hm.move_next == null)
					return false;
				
				hm.get_current = FetchMethodGetCurrent (return_type);
				if (hm.get_current == null)
					return false;
			}
			
			hm.element_type = hm.get_current.ReturnType;
			hm.enumerator_type = return_type;
			hm.is_disposable = !hm.enumerator_type.IsSealed ||
				TypeManager.ImplementsInterface (
					hm.enumerator_type, TypeManager.idisposable_type);

			return true;
		}
		
		/// <summary>
		///   This filter is used to find the GetEnumerator method
		///   on which IEnumerator operates
		/// </summary>
		static MemberFilter FilterEnumerator;
		
		static Foreach ()
		{
			FilterEnumerator = new MemberFilter (GetEnumeratorFilter);
		}

                void error1579 (Type t)
                {
                        Report.Error (1579, loc,
                                      "foreach statement cannot operate on variables of type `" +
                                      t.FullName + "' because that class does not provide a " +
                                      " GetEnumerator method or it is inaccessible");
                }

		static bool TryType (Type t, ForeachHelperMethods hm)
		{
			MemberList mi;
			
			mi = TypeContainer.FindMembers (t, MemberTypes.Method,
							BindingFlags.Public | BindingFlags.NonPublic |
							BindingFlags.Instance | BindingFlags.DeclaredOnly,
							FilterEnumerator, hm);

			if (mi.Count == 0)
				return false;

			hm.get_enumerator = (MethodInfo) mi [0];
			return true;	
		}
		
		//
		// Looks for a usable GetEnumerator in the Type, and if found returns
		// the three methods that participate: GetEnumerator, MoveNext and get_Current
		//
		ForeachHelperMethods ProbeCollectionType (EmitContext ec, Type t)
		{
			ForeachHelperMethods hm = new ForeachHelperMethods (ec);

			for (Type tt = t; tt != null && tt != TypeManager.object_type;){
				if (TryType (tt, hm))
					return hm;
				tt = tt.BaseType;
			}

			//
			// Now try to find the method in the interfaces
			//
			while (t != null){
				Type [] ifaces = t.GetInterfaces ();

				foreach (Type i in ifaces){
					if (TryType (i, hm))
						return hm;
				}
				
				//
				// Since TypeBuilder.GetInterfaces only returns the interface
				// types for this type, we have to keep looping, but once
				// we hit a non-TypeBuilder (ie, a Type), then we know we are
				// done, because it returns all the types
				//
				if ((t is TypeBuilder))
					t = t.BaseType;
				else
					break;
			} 

			return null;
		}

		//
		// FIXME: possible optimization.
		// We might be able to avoid creating `empty' if the type is the sam
		//
		bool EmitCollectionForeach (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			enumerator = new VariableStorage (ec, hm.enumerator_type);
			enumerator.EmitThis (ig);
			//
			// Instantiate the enumerator
			//
			if (expr.Type.IsValueType){
				IMemoryLocation ml = expr as IMemoryLocation;
				// Load the address of the value type.
				if (ml == null) {
					// This happens if, for example, you have a property
					// returning a struct which is IEnumerable
					LocalBuilder t = ec.GetTemporaryLocal (expr.Type);
						expr.Emit(ec);
					ig.Emit (OpCodes.Stloc, t);
					ig.Emit (OpCodes.Ldloca, t);
					ec.FreeTemporaryLocal (t, expr.Type);
					} else {
						ml.AddressOf (ec, AddressOp.Load);
					}
				
				// Emit the call.
				if (hm.get_enumerator.DeclaringType.IsValueType) {
					// the method is declared on the value type
					ig.Emit (OpCodes.Call, hm.get_enumerator);
				} else {
					// it is an interface method, so we must box
					ig.Emit (OpCodes.Box, expr.Type);
				ig.Emit (OpCodes.Callvirt, hm.get_enumerator);
				}
			} else {
				expr.Emit (ec);
				ig.Emit (OpCodes.Callvirt, hm.get_enumerator);
			}
			enumerator.EmitStore (ig);

			//
			// Protect the code in a try/finalize block, so that
			// if the beast implement IDisposable, we get rid of it
			//
			if (hm.is_disposable && emit_finally)
				ig.BeginExceptionBlock ();
			
			Label end_try = ig.DefineLabel ();
			
			ig.MarkLabel (ec.LoopBegin);
			
			enumerator.EmitCall (ig, hm.move_next);
			
			ig.Emit (OpCodes.Brfalse, end_try);

			if (ec.InIterator)
				ig.Emit (OpCodes.Ldarg_0);
			
			enumerator.EmitCall (ig, hm.get_current);

			if (ec.InIterator){
				conv.Emit (ec);
				ig.Emit (OpCodes.Stfld, ((LocalVariableReference) variable).local_info.FieldBuilder);
			} else 
				((IAssignMethod)variable).EmitAssign (ec, conv, false, false);
				
			statement.Emit (ec);
			ig.Emit (OpCodes.Br, ec.LoopBegin);
			ig.MarkLabel (end_try);
			
			// The runtime provides this for us.
			// ig.Emit (OpCodes.Leave, end);

			//
			// Now the finally block
			//
			if (hm.is_disposable) {
				DoEmitFinally (ec);
				if (emit_finally)
					ig.EndExceptionBlock ();
			}

			ig.MarkLabel (ec.LoopEnd);
			return false;
		}

		public override void EmitFinally (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (hm.enumerator_type.IsValueType) {
				enumerator.EmitThis (ig);

				MethodInfo mi = FetchMethodDispose (hm.enumerator_type);
				if (mi != null) {
					enumerator.EmitLoadAddress (ig);
					ig.Emit (OpCodes.Call, mi);
				} else {
					enumerator.EmitLoad (ig);
					ig.Emit (OpCodes.Box, hm.enumerator_type);
					ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				}
			} else {
				Label call_dispose = ig.DefineLabel ();

				enumerator.EmitThis (ig);
				enumerator.EmitLoad (ig);
				ig.Emit (OpCodes.Isinst, TypeManager.idisposable_type);
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Brtrue_S, call_dispose);
				ig.Emit (OpCodes.Pop);

				Label end_finally = ig.DefineLabel ();
				ig.Emit (OpCodes.Br, end_finally);

				ig.MarkLabel (call_dispose);
				ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				ig.MarkLabel (end_finally);

				if (emit_finally)
					ig.Emit (OpCodes.Endfinally);
			}
		}

		//
		// FIXME: possible optimization.
		// We might be able to avoid creating `empty' if the type is the sam
		//
		bool EmitArrayForeach (EmitContext ec)
		{
			int rank = array_type.GetArrayRank ();
			ILGenerator ig = ec.ig;

			VariableStorage copy = new VariableStorage (ec, array_type);
			
			//
			// Make our copy of the array
			//
			copy.EmitThis (ig);
			expr.Emit (ec);
			copy.EmitStore (ig);
			
			if (rank == 1){
				VariableStorage counter = new VariableStorage (ec,TypeManager.int32_type);

				Label loop, test;

				counter.EmitThis (ig);
				ig.Emit (OpCodes.Ldc_I4_0);
				counter.EmitStore (ig);
				test = ig.DefineLabel ();
				ig.Emit (OpCodes.Br, test);

				loop = ig.DefineLabel ();
				ig.MarkLabel (loop);

				if (ec.InIterator)
					ig.Emit (OpCodes.Ldarg_0);
				
				copy.EmitThis (ig);
				copy.EmitLoad (ig);
				counter.EmitThis (ig);
				counter.EmitLoad (ig);

				//
				// Load the value, we load the value using the underlying type,
				// then we use the variable.EmitAssign to load using the proper cast.
				//
				ArrayAccess.EmitLoadOpcode (ig, element_type);
				if (ec.InIterator){
					conv.Emit (ec);
					ig.Emit (OpCodes.Stfld, ((LocalVariableReference) variable).local_info.FieldBuilder);
				} else 
					((IAssignMethod)variable).EmitAssign (ec, conv, false, false);

				statement.Emit (ec);

				ig.MarkLabel (ec.LoopBegin);
				counter.EmitThis (ig);
				counter.EmitThis (ig);
				counter.EmitLoad (ig);
				ig.Emit (OpCodes.Ldc_I4_1);
				ig.Emit (OpCodes.Add);
				counter.EmitStore (ig);

				ig.MarkLabel (test);
				counter.EmitThis (ig);
				counter.EmitLoad (ig);
				copy.EmitThis (ig);
				copy.EmitLoad (ig);
				ig.Emit (OpCodes.Ldlen);
				ig.Emit (OpCodes.Conv_I4);
				ig.Emit (OpCodes.Blt, loop);
			} else {
				VariableStorage [] dim_len   = new VariableStorage [rank];
				VariableStorage [] dim_count = new VariableStorage [rank];
				Label [] loop = new Label [rank];
				Label [] test = new Label [rank];
				int dim;
				
				for (dim = 0; dim < rank; dim++){
					dim_len [dim] = new VariableStorage (ec, TypeManager.int32_type);
					dim_count [dim] = new VariableStorage (ec, TypeManager.int32_type);
					test [dim] = ig.DefineLabel ();
					loop [dim] = ig.DefineLabel ();
				}
					
				for (dim = 0; dim < rank; dim++){
					dim_len [dim].EmitThis (ig);
					copy.EmitThis (ig);
					copy.EmitLoad (ig);
					IntLiteral.EmitInt (ig, dim);
					ig.Emit (OpCodes.Callvirt, TypeManager.int_getlength_int);
					dim_len [dim].EmitStore (ig);
					
				}

				for (dim = 0; dim < rank; dim++){
					dim_count [dim].EmitThis (ig);
					ig.Emit (OpCodes.Ldc_I4_0);
					dim_count [dim].EmitStore (ig);
					ig.Emit (OpCodes.Br, test [dim]);
					ig.MarkLabel (loop [dim]);
				}

				if (ec.InIterator)
					ig.Emit (OpCodes.Ldarg_0);
				
				copy.EmitThis (ig);
				copy.EmitLoad (ig);
				for (dim = 0; dim < rank; dim++){
					dim_count [dim].EmitThis (ig);
					dim_count [dim].EmitLoad (ig);
				}

				//
				// FIXME: Maybe we can cache the computation of `get'?
				//
				Type [] args = new Type [rank];
				MethodInfo get;

				for (int i = 0; i < rank; i++)
					args [i] = TypeManager.int32_type;

				ModuleBuilder mb = CodeGen.Module.Builder;
				get = mb.GetArrayMethod (
					array_type, "Get",
					CallingConventions.HasThis| CallingConventions.Standard,
					var_type, args);
				ig.Emit (OpCodes.Call, get);
				if (ec.InIterator){
					conv.Emit (ec);
					ig.Emit (OpCodes.Stfld, ((LocalVariableReference) variable).local_info.FieldBuilder);
				} else 
					((IAssignMethod)variable).EmitAssign (ec, conv, false, false);
				statement.Emit (ec);
				ig.MarkLabel (ec.LoopBegin);
				for (dim = rank - 1; dim >= 0; dim--){
					dim_count [dim].EmitThis (ig);
					dim_count [dim].EmitThis (ig);
					dim_count [dim].EmitLoad (ig);
					ig.Emit (OpCodes.Ldc_I4_1);
					ig.Emit (OpCodes.Add);
					dim_count [dim].EmitStore (ig);

					ig.MarkLabel (test [dim]);
					dim_count [dim].EmitThis (ig);
					dim_count [dim].EmitLoad (ig);
					dim_len [dim].EmitThis (ig);
					dim_len [dim].EmitLoad (ig);
					ig.Emit (OpCodes.Blt, loop [dim]);
				}
			}
			ig.MarkLabel (ec.LoopEnd);
			
			return false;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			Label old_begin = ec.LoopBegin, old_end = ec.LoopEnd;
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.PushVBLoop (VBLoopKind.For, ec.LoopBegin, ec.LoopEnd);
			
			if (hm != null)
				EmitCollectionForeach (ec);
			else
				EmitArrayForeach (ec);

			ec.PopVBLoop ();
			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}
	}

	//
	// VB.NET's Select Case is NOT a C-style switch.  Case
	// clauses can be arbitrary runtime expressions, ranges
	// (Case x To y) or comparisons (Case Is > x), and the
	// selector is evaluated exactly once even when multiple
	// case clauses reference it.  See the
	// mono-basic/vbnc/.../CaseClause.ResolveCode reference
	// implementation for the semantics we follow.
	//
	// We lower Select Case to: evaluate selector into a
	// synthetic LocalBuilder, then run an if/elseif chain
	// where each case section's condition is the disjunction
	// of its case-clause predicates against that cached
	// selector, with Case Else producing the final 'else'.
	//
	// This lowering is performed at Resolve() time rather
	// than in the parser because the selector's type isn't
	// known until semantic analysis.  Using a synthetic
	// Object-typed local in the parser would force the
	// comparisons through bmcs's Object-operand runtime
	// helper path (ObjectType.ObjTst), which is broken in
	// the mono 1.2.6 Microsoft.VisualBasic we bootstrap
	// against and produces wrong results for integer and
	// string comparisons.
	//
	public class VBCaseClause {
		public const int K_Equal   = 0; // Case expr
		public const int K_Range   = 1; // Case lo To hi
		public const int K_Compare = 2; // Case [Is] <op> expr

		public int Kind;
		public Expression Expr1;
		public Expression Expr2;
		public Binary.Operator CompareOp;

		public VBCaseClause (int kind, Expression e1, Expression e2, Binary.Operator op)
		{
			Kind = kind;
			Expr1 = e1;
			Expr2 = e2;
			CompareOp = op;
		}
	}

	public class VBSelectSection {
		// null Clauses means 'Case Else'
		public ArrayList Clauses;
		public Block Body;

		public VBSelectSection (ArrayList clauses, Block body)
		{
			Clauses = clauses;
			Body = body;
		}
	}

	public class VBSelect : Statement {
		Expression Selector;
		ArrayList Sections;

		// Filled in by Resolve():
		Expression selector_resolved;
		LocalBuilder temp_builder;
		Type temp_type;
		Statement chain;

		public VBSelect (Expression selector, ArrayList sections, Location l)
		{
			Selector = selector;
			Sections = sections;
			loc = l;
		}

		// A fresh LocalTemporary per reference so nothing is
		// shared between the multiple Binary operands that
		// load the cached selector.
		Expression MakeTempRef ()
		{
			return new LocalTemporary (temp_builder, temp_type);
		}

		Expression BuildPredicate (VBCaseClause c)
		{
			switch (c.Kind) {
			case VBCaseClause.K_Equal:
				return new Binary (Binary.Operator.Equality,
					MakeTempRef (), c.Expr1, loc);
			case VBCaseClause.K_Range: {
				Expression lo_p = new Binary (
					Binary.Operator.GreaterThanOrEqual,
					MakeTempRef (), c.Expr1, loc);
				Expression hi_p = new Binary (
					Binary.Operator.LessThanOrEqual,
					MakeTempRef (), c.Expr2, loc);
				return new Binary (
					Binary.Operator.LogicalAndAlso,
					lo_p, hi_p, loc);
			}
			case VBCaseClause.K_Compare:
				return new Binary (c.CompareOp,
					MakeTempRef (), c.Expr1, loc);
			}
			throw new Exception ("VBSelect: unknown VBCaseClause kind");
		}

		public override bool Resolve (EmitContext ec)
		{
			selector_resolved = Selector.Resolve (ec);
			if (selector_resolved == null)
				return false;

			temp_type = selector_resolved.Type;
			// Allocate the backing local once.  Fresh
			// LocalTemporary wrappers below will all reference
			// it, so Store/Emit always hit the same slot.
			temp_builder = ec.ig.DeclareLocal (temp_type);

			// Build the if/elseif chain from back to front so
			// each step wraps the previously-built 'else' branch.
			Statement tail = null;
			for (int i = Sections.Count - 1; i >= 0; i--) {
				VBSelectSection sec = (VBSelectSection) Sections [i];

				if (sec.Clauses == null) {
					// The parser keeps Case Else, if present,
					// as the final section only.
					tail = sec.Body;
					continue;
				}

				// OR the case-clause predicates together.
				Expression pred = null;
				foreach (VBCaseClause cc in sec.Clauses) {
					Expression p = BuildPredicate (cc);
					pred = pred == null
						? p
						: new Binary (
							Binary.Operator.LogicalOrElse,
							pred, p, loc);
				}

				tail = tail == null
					? new If (pred, sec.Body, loc)
					: new If (pred, sec.Body, tail, loc);
			}

			chain = tail;

			if (chain != null && !chain.Resolve (ec))
				return false;

			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			// Evaluate the selector exactly once, cache it.
			selector_resolved.Emit (ec);
			ig.Emit (OpCodes.Stloc, temp_builder);

			if (chain != null)
				chain.Emit (ec);
		}
	}

	public class VBReDimClause {
		public readonly Expression Target;
		public readonly ArrayList Bounds;

		public VBReDimClause (Expression target, ArrayList bounds)
		{
			Target = target;
			Bounds = bounds;
		}
	}

	public class VBReDim : Statement {
		ArrayList clauses;
		bool preserve;
		ArrayList resolved;

		class Resolved {
			public Expression Target;
			public Expression OldSource;
			public Expression Source;
			public Type ArrayType;
		}

		static MethodInfo array_copy_method;
		static MethodInfo array_length_getter;
		static MethodInfo math_min_method;

		public VBReDim (ArrayList clauses, bool preserve, Location l)
		{
			this.clauses = clauses;
			this.preserve = preserve;
			loc = l;
		}

		static void EnsurePreserveHelpers ()
		{
			if (array_copy_method != null)
				return;

			Type array_type = typeof (System.Array);
			array_copy_method = array_type.GetMethod ("Copy",
				new Type [] { array_type, array_type, typeof (int) });
			array_length_getter = array_type.GetProperty ("Length").GetGetMethod ();
			math_min_method = typeof (System.Math).GetMethod ("Min",
				new Type [] { typeof (int), typeof (int) });
		}

		public override bool Resolve (EmitContext ec)
		{
			resolved = new ArrayList ();

			foreach (VBReDimClause clause in clauses) {
				Expression target = clause.Target.ResolveLValue (ec, new EmptyExpression ());
				if (target == null)
					return false;

				if (target.Type == TypeManager.object_type) {
					Report.Error (30828, loc,
						"'ReDim' on Object-typed targets is not supported by this bootstrap compiler");
					return false;
				}

				if (!target.Type.IsArray) {
					Report.Error (30049, loc,
						"'ReDim' statement requires an array target");
					return false;
				}

				int rank = target.Type.GetArrayRank ();
				if (clause.Bounds.Count != rank) {
					Report.Error (30415, loc,
						"'ReDim' bound count ({0}) does not match array rank ({1})",
						clause.Bounds.Count, rank);
					return false;
				}

				if (preserve) {
					if (rank != 1) {
						Report.Error (30829, loc,
							"'ReDim Preserve' is only supported for one-dimensional arrays by this bootstrap compiler");
						return false;
					}

					if (!(target is LocalVariableReference) &&
					    !(target is ParameterReference)) {
						Report.Error (30827, loc,
							"'ReDim Preserve' is only supported on simple local or parameter arrays by this bootstrap compiler");
						return false;
					}
				}

				ArrayList resolved_bounds = new ArrayList (rank);
				foreach (Expression bound in clause.Bounds) {
					Expression rb = bound.Resolve (ec);
					if (rb == null)
						return false;

					rb = Convert.WideningConversionRequired (ec, rb, TypeManager.int32_type, loc);
					if (rb == null)
						return false;

					resolved_bounds.Add (rb);
				}

				Expression source = new VBReDimNewArray (
					TypeManager.GetElementType (target.Type), target.Type, resolved_bounds, loc);
				source = source.Resolve (ec);
				if (source == null)
					return false;

				Resolved r = new Resolved ();
				r.Target = target;
				r.OldSource = preserve ? target : null;
				r.Source = source;
				r.ArrayType = target.Type;
				resolved.Add (r);
			}

			if (preserve)
				EnsurePreserveHelpers ();

			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			foreach (Resolved r in resolved) {
				if (!preserve) {
					Assign assign = new Assign (r.Target, r.Source, loc);
					ExpressionStatement es = (ExpressionStatement) assign.Resolve (ec);
					if (es != null)
						es.EmitStatement (ec);
					continue;
				}

				LocalTemporary old_temp = new LocalTemporary (ec, r.ArrayType);
				r.OldSource.Emit (ec);
				old_temp.Store (ec);

				LocalTemporary new_temp = new LocalTemporary (ec, r.ArrayType);
				r.Source.Emit (ec);
				new_temp.Store (ec);

				Label skip_copy = ig.DefineLabel ();
				old_temp.Emit (ec);
				ig.Emit (OpCodes.Brfalse, skip_copy);

				old_temp.Emit (ec);
				new_temp.Emit (ec);
				old_temp.Emit (ec);
				ig.Emit (OpCodes.Callvirt, array_length_getter);
				new_temp.Emit (ec);
				ig.Emit (OpCodes.Callvirt, array_length_getter);
				ig.Emit (OpCodes.Call, math_min_method);
				ig.Emit (OpCodes.Call, array_copy_method);
				ig.MarkLabel (skip_copy);

				Assign preserve_assign = new Assign (r.Target, new_temp, loc);
				ExpressionStatement pes = (ExpressionStatement) preserve_assign.Resolve (ec);
				if (pes != null)
					pes.EmitStatement (ec);
			}
		}
	}

	public class VBReDimNewArray : Expression {
		Type element_type;
		ArrayList bounds;
		MethodInfo multi_ctor;

		public VBReDimNewArray (Type element_type, Type array_type, ArrayList bounds, Location loc)
		{
			this.element_type = element_type;
			this.bounds = bounds;
			this.type = array_type;
			this.loc = loc;
			this.eclass = ExprClass.Value;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (bounds.Count > 1) {
				ModuleBuilder mb = CodeGen.Module.Builder;
				Type[] arg_types = new Type [bounds.Count];
				for (int i = 0; i < arg_types.Length; i++)
					arg_types [i] = TypeManager.int32_type;

				multi_ctor = mb.GetArrayMethod (type, ".ctor",
					CallingConventions.HasThis, null, arg_types);
				if (multi_ctor == null) {
					Report.Error (-6, loc,
						"ReDim: cannot find multi-dimensional array constructor for " +
						TypeManager.CSharpName (type));
					return null;
				}
			}

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (multi_ctor == null) {
				((Expression) bounds [0]).Emit (ec);
				ig.Emit (OpCodes.Ldc_I4_1);
				ig.Emit (OpCodes.Add);
				ig.Emit (OpCodes.Newarr, element_type);
				return;
			}

			foreach (Expression bound in bounds) {
				bound.Emit (ec);
				ig.Emit (OpCodes.Ldc_I4_1);
				ig.Emit (OpCodes.Add);
			}

			ig.Emit (OpCodes.Newobj, multi_ctor);
		}
	}
}
