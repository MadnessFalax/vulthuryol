using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Lab7
{
    public class EvalVisitor : PLC_Lab7_exprBaseVisitor<(Type Type, object Value)>
    {
        SymbolTable symbolTable = new SymbolTable();
        public override (Type Type, object Value) VisitInt([NotNull] PLC_Lab7_exprParser.IntContext context)
        {
            return (Type.INT, int.Parse(context.INT().GetText()));
        }
        public override (Type Type, object Value) VisitFloat([NotNull] PLC_Lab7_exprParser.FloatContext context)
        {
            return (Type.FLOAT, float.Parse(context.FLOAT().GetText()));
        }
        public override (Type Type, object Value) VisitBool([NotNull] PLC_Lab7_exprParser.BoolContext context)
        {
            return (Type.BOOL, bool.Parse(context.BOOL().GetText()));
        }
        public override (Type Type, object Value) VisitString([NotNull] PLC_Lab7_exprParser.StringContext context)
        {
            return (Type.STRING, context.STRING().GetText());
        }
        public override (Type Type, object Value) VisitId([NotNull] PLC_Lab7_exprParser.IdContext context)
        {
            if (context.ID().Symbol.Text == "true")
                return (Type.BOOL, true);
            else if (context.ID().Symbol.Text == "false")
                return (Type.BOOL, false);
            
            return symbolTable[context.ID().Symbol];
        }
        public override (Type Type, object Value) VisitType_kw([NotNull] PLC_Lab7_exprParser.Type_kwContext context)
        {
            if (context.type.Text.Equals("int")) return (Type.INT, 0);
            else if (context.type.Text.Equals("float")) return (Type.FLOAT, 0);
            else if (context.type.Text.Equals("bool")) return (Type.BOOL, false);
            else if (context.type.Text.Equals("string")) return (Type.STRING, "");
            else return (Type.ERROR, 0);
        }
        public override (Type Type, object Value) VisitProg([NotNull] PLC_Lab7_exprParser.ProgContext context)
        {
            foreach (var statement in context.statement())
            {
                Visit(statement);
            }
            return (Type.ERROR, 0);
        }
        public override (Type Type, object Value) VisitEmptyStatement([NotNull] PLC_Lab7_exprParser.EmptyStatementContext context)
        {
            return (Type.ERROR, 0);
        }

        public override (Type Type, object Value) VisitDeclaration([NotNull] PLC_Lab7_exprParser.DeclarationContext context)
        {
            var type = Visit(context.type_kw());
            foreach (var id in context.ID())
            {
                symbolTable.Add(id.Symbol, type.Type);
            }
            return (Type.ERROR, 0);
        }
        public override (Type Type, object Value) VisitExpression([NotNull] PLC_Lab7_exprParser.ExpressionContext context)
        {
            Visit(context.expr());
            return (Type.ERROR, 0);
        }
        public override (Type Type, object Value) VisitReadCLI([NotNull] PLC_Lab7_exprParser.ReadCLIContext context)
        {
            foreach (var id in context.ID())
            {
                Visit(id);
            }
            return (Type.ERROR, 0);
        }
        public override (Type Type, object Value) VisitWriteCLI([NotNull] PLC_Lab7_exprParser.WriteCLIContext context)
        {
            foreach (var expr in context.expr())
            {
                Visit(expr);
            }
            return (Type.ERROR, 0);
        }
        public override (Type Type, object Value) VisitCodeBlock([NotNull] PLC_Lab7_exprParser.CodeBlockContext context)
        {
            foreach (var statement in context.statement())
            {
                Visit(statement);
            }
            return (Type.ERROR, 0);
        }
        public override (Type Type, object Value) VisitIfStatement([NotNull] PLC_Lab7_exprParser.IfStatementContext context)
        {
            Visit(context.condition());
            foreach (var statement in context.statement())
            {
                Visit(statement);
            }
            return (Type.ERROR, 0);
        }
        public override (Type Type, object Value) VisitWhileStatement([NotNull] PLC_Lab7_exprParser.WhileStatementContext context)
        {
            Visit(context.condition());
            Visit(context.statement());
            return (Type.ERROR, 0);
        }
        public override (Type Type, object Value) VisitConditionalExpr([NotNull] PLC_Lab7_exprParser.ConditionalExprContext context)
        {
            var cond = Visit(context.expr());
            if (cond.Type != Type.BOOL)
            {
                Errors.ReportError(context.expr().Start, $"Condition {context.expr().GetText()} must be bool. Instead is {cond.Type.ToString()}");
            }
            return (Type.ERROR, 0);
        }
        public override (Type Type, object Value) VisitUnaryMinus([NotNull] PLC_Lab7_exprParser.UnaryMinusContext context)
        {
            var operand = Visit(context.expr());
            if (operand.Type != Type.INT && operand.Type != Type.FLOAT)
            {
                Errors.ReportError(context.expr().Start, $"Unary minus operand {context.expr().GetText()} must be int or float. Instead is {operand.Type.ToString()}");
                return (Type.ERROR, 0);
            }
            return operand;
        }
        public override (Type Type, object Value) VisitUnaryNeg([NotNull] PLC_Lab7_exprParser.UnaryNegContext context)
        {
            var operand = Visit(context.expr());
            if (operand.Type != Type.BOOL)
            {
                Errors.ReportError(context.expr().Start, $"Unary minus operand {context.expr().GetText()} must be bool. Instead is {operand.Type.ToString()}");
                return (Type.ERROR, 0);
            }
            return operand;
        }
        public override (Type Type, object Value) VisitExpr([NotNull] PLC_Lab7_exprParser.ExprContext context)
        {
            return Visit(context.assignment_expr());
        }

        public override (Type Type, object Value) VisitNestedAss([NotNull] PLC_Lab7_exprParser.NestedAssContext context)
        {
            var lhs = symbolTable[context.ID().Symbol];
            var rhs = Visit(context.assignment_expr());
            if (lhs.Type != rhs.Type)
            {
                Errors.ReportError(context.assignment_expr().Start, $"Right side has different type than {context.ID().GetText()}. Left side is {lhs.Type.ToString()}. Right side is {rhs.Type.ToString()}.");
                return (Type.ERROR, 0);
            }
            return lhs;
        }

        public override (Type Type, object Value) VisitLeavingAss([NotNull] PLC_Lab7_exprParser.LeavingAssContext context)
        {
            return Visit(context.assignment_leaf());
        }

        public override (Type Type, object Value) VisitNestedOr([NotNull] PLC_Lab7_exprParser.NestedOrContext context)
        {
            var lhs = Visit(context.or_expr());
            var rhs = Visit(context.or_leaf());
            if (lhs.Type != rhs.Type)
            {
                Errors.ReportError(context.or_expr().Start, $"Right side has different type than left side. Left side is {lhs.Type.ToString()}. Right side is {rhs.Type.ToString()}.");
                return (Type.ERROR, 0);
            }
            if (lhs.Type != Type.BOOL || lhs.Type != Type.BOOL)
            {
                Errors.ReportError(context.or_expr().Start, $"Both side operands should be bool type. Current left: {lhs.Type}. Current right: {rhs.Type}");
                return (Type.ERROR, 0);
            }
            return lhs;
        }

        public override (Type Type, object Value) VisitLeavingOr([NotNull] PLC_Lab7_exprParser.LeavingOrContext context)
        {
            return Visit(context.or_leaf());
        }
        public override (Type Type, object Value) VisitNestedAnd([NotNull] PLC_Lab7_exprParser.NestedAndContext context)
        {
            var lhs = Visit(context.and_expr());
            var rhs = Visit(context.and_leaf());
            if (lhs.Type != rhs.Type)
            {
                Errors.ReportError(context.and_expr().Start, $"Right side has different type than left side. Left side is {lhs.Type.ToString()}. Right side is {rhs.Type.ToString()}.");
                return (Type.ERROR, 0);
            }
            if (lhs.Type != Type.BOOL || lhs.Type != Type.BOOL)
            {
                Errors.ReportError(context.and_expr().Start, $"Both side operands should be bool type. Current left: {lhs.Type}. Current right: {rhs.Type}");
                return (Type.ERROR, 0);
            }
            return lhs;
        }

        public override (Type Type, object Value) VisitLeavingAnd([NotNull] PLC_Lab7_exprParser.LeavingAndContext context)
        {
            return Visit(context.and_leaf());
        }
        public override (Type Type, object Value) VisitNestedComp([NotNull] PLC_Lab7_exprParser.NestedCompContext context)
        {
            var lhs = Visit(context.comp_expr());
            var rhs = Visit(context.comp_leaf());
            if (lhs.Type != rhs.Type)
            {
                Errors.ReportError(context.comp_expr().Start, $"Right side has different type than left side. Left side is {lhs.Type.ToString()}. Right side is {rhs.Type.ToString()}.");
                return (Type.ERROR, 0);
            }
            if (lhs.Type == Type.BOOL || lhs.Type == Type.BOOL || lhs.Type == Type.ERROR || rhs.Type == Type.ERROR)
            {
                Errors.ReportError(context.comp_expr().Start, $"Both side operands should be int, float or string type. Current left: {lhs.Type}. Current right: {rhs.Type}");
                return (Type.ERROR, 0);
            }
            return (Type.BOOL, false);
        }

        public override (Type Type, object Value) VisitLeavingComp([NotNull] PLC_Lab7_exprParser.LeavingCompContext context)
        {
            return Visit(context.comp_leaf());
        }
        public override (Type Type, object Value) VisitNestedRel([NotNull] PLC_Lab7_exprParser.NestedRelContext context)
        {
            var lhs = Visit(context.rel_expr());
            var rhs = Visit(context.rel_leaf());
            if (lhs.Type != rhs.Type)
            {
                if (lhs.Type == Type.FLOAT && rhs.Type == Type.INT || lhs.Type == Type.INT && rhs.Type == Type.FLOAT )
                {
                    return (Type.FLOAT, 0.0);
                }
                Errors.ReportError(context.rel_expr().Start, $"Right side has different type than left side. Left side is {lhs.Type.ToString()}. Right side is {rhs.Type.ToString()}.");
                return (Type.ERROR, 0);
            }
            if (lhs.Type != Type.INT && lhs.Type != Type.FLOAT || rhs.Type != Type.INT && rhs.Type != Type.FLOAT)
            {
                Errors.ReportError(context.rel_expr().Start, $"Both side operands should be int or float. Current left: {lhs.Type}. Current right: {rhs.Type}");
                return (Type.ERROR, 0);
            }
            return (Type.BOOL, false);
        }

        public override (Type Type, object Value) VisitLeavingRel([NotNull] PLC_Lab7_exprParser.LeavingRelContext context)
        {
            return Visit(context.rel_leaf());
        }
        public override (Type Type, object Value) VisitNestedAdd([NotNull] PLC_Lab7_exprParser.NestedAddContext context)
        {
            var lhs = Visit(context.add_expr());
            var rhs = Visit(context.add_leaf());
            if (context.op.Type == PLC_Lab7_exprParser.ADD_OP || context.op.Type == PLC_Lab7_exprParser.MIN_OP)
            {
                if (lhs.Type == rhs.Type)
                {
                    if (lhs.Type == Type.INT)
                    {
                        return lhs;
                    }
                    if (lhs.Type == Type.FLOAT)
                    {
                        return lhs;
                    }
                }
                if (lhs.Type == Type.INT && rhs.Type == Type.FLOAT) {
                    return rhs;
                }
                if (lhs.Type == Type.FLOAT && rhs.Type == Type.INT)
                {
                    return lhs;
                }
                Errors.ReportError(context.add_expr().Start, $"Both + or - operators should be either INT or FLOAT type. Left is: {lhs.Type}. Right is: {rhs.Type}");
                return (Type.ERROR, 0);
            }
            else if (context.op.Type == PLC_Lab7_exprParser.CONCAT_OP)
            {
                if (lhs.Type == rhs.Type && lhs.Type == Type.STRING)
                {
                    return (Type.STRING, true);
                }
                Errors.ReportError(context.add_expr().Start, $"Both concatenation operands should be of type string. Left: {lhs.Type.ToString()}. Right: {rhs.Type.ToString()} ");
                return (Type.ERROR, 0);
            }
            return (Type.ERROR, 0);
        }

        public override (Type Type, object Value) VisitLeavingAdd([NotNull] PLC_Lab7_exprParser.LeavingAddContext context)
        {
            return Visit(context.add_leaf());
        }
        public override (Type Type, object Value) VisitNestedMul([NotNull] PLC_Lab7_exprParser.NestedMulContext context)
        {
            var lhs = Visit(context.mul_expr());
            var rhs = Visit(context.mul_leaf());
            if (context.op.Type == PLC_Lab7_exprParser.MUL_OP || context.op.Type == PLC_Lab7_exprParser.DIV_OP )
            {
                if (lhs.Type == rhs.Type)
                {
                    if (lhs.Type == Type.INT)
                    {
                        return lhs;
                    }
                    if (lhs.Type == Type.FLOAT)
                    {
                        return lhs;
                    }
                }
                if (lhs.Type == Type.INT && rhs.Type == Type.FLOAT)
                {
                    return rhs;
                }
                if (lhs.Type == Type.FLOAT && rhs.Type == Type.INT)
                {
                    return lhs;
                }
                Errors.ReportError(context.mul_expr().Start, $"Both * or / operators should be either INT or FLOAT type. Left is: {lhs.Type}. Right is: {rhs.Type}");
                return (Type.ERROR, 0);
            }
            else if (context.op.Type == PLC_Lab7_exprParser.MOD_OP) 
            {
                if (lhs.Type == rhs.Type && lhs.Type == Type.INT)
                {
                    return lhs;
                }
                Errors.ReportError(context.mul_expr().Start, $"Modulo operators should be INT type. Left is: {lhs.Type}. Right is: {rhs.Type}");
                return (Type.ERROR, 0);
            }
            return (Type.ERROR, 0);
        }

        public override (Type Type, object Value) VisitLeavingMul([NotNull] PLC_Lab7_exprParser.LeavingMulContext context)
        {
            return Visit(context.mul_leaf());
        }

        public override (Type Type, object Value) VisitCommonUnnary([NotNull] PLC_Lab7_exprParser.CommonUnnaryContext context)
        {
            return Visit(context.unnary_expr());
        }

        public override (Type Type, object Value) VisitCommonParent([NotNull] PLC_Lab7_exprParser.CommonParentContext context)
        {
            return Visit(context.expr());
        }
        public override (Type Type, object Value) VisitCommonLeaf([NotNull] PLC_Lab7_exprParser.CommonLeafContext context)
        {
            return Visit(context.leaf());
        }
        public override (Type Type, object Value) VisitMulTerminal([NotNull] PLC_Lab7_exprParser.MulTerminalContext context)
        {
            return Visit(context.leaf_common());
        }

        public override (Type Type, object Value) VisitAddTerminal([NotNull] PLC_Lab7_exprParser.AddTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, object Value) VisitAddToMul([NotNull] PLC_Lab7_exprParser.AddToMulContext context)
        {
            return Visit(context.mul_expr());
        }
        public override (Type Type, object Value) VisitRelTerminal([NotNull] PLC_Lab7_exprParser.RelTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, object Value) VisitRelToAdd([NotNull] PLC_Lab7_exprParser.RelToAddContext context)
        {
            return Visit(context.add_expr());
        }
        public override (Type Type, object Value) VisitCompTerminal([NotNull] PLC_Lab7_exprParser.CompTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, object Value) VisitCompToRel([NotNull] PLC_Lab7_exprParser.CompToRelContext context)
        {
            return Visit(context.rel_expr());
        }
        public override (Type Type, object Value) VisitAndTerminal([NotNull] PLC_Lab7_exprParser.AndTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, object Value) VisitAndToComp([NotNull] PLC_Lab7_exprParser.AndToCompContext context)
        {
            return Visit(context.comp_expr());
        }
        public override (Type Type, object Value) VisitOrTerminal([NotNull] PLC_Lab7_exprParser.OrTerminalContext context)
        {
            return Visit(context.leaf_common());
        }

        public override (Type Type, object Value) VisitOrToAnd([NotNull] PLC_Lab7_exprParser.OrToAndContext context)
        {
            return Visit(context.and_expr());
        }
        public override (Type Type, object Value) VisitAssTerminal([NotNull] PLC_Lab7_exprParser.AssTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, object Value) VisitAssToOr([NotNull] PLC_Lab7_exprParser.AssToOrContext context)
        {
            return Visit(context.or_expr());
        }
        /*
        public override int VisitInt([NotNull] PLC_Lab7_exprParser.IntContext context)
        {
            return Convert.ToInt32(context.INT().GetText(), 10);
        }
        public override int VisitHexa([NotNull] PLC_Lab7_exprParser.HexaContext context)
        {
            return Convert.ToInt32(context.HEXA().GetText(), 16);
        }
        public override int VisitOct([NotNull] PLC_Lab7_exprParser.OctContext context)
        {
            return Convert.ToInt32(context.OCT().GetText(), 8);
        }
        public override int VisitPar([NotNull] PLC_Lab7_exprParser.ParContext context)
        {
            return Visit(context.expr());
        }
        public override int VisitAdd([NotNull] PLC_Lab7_exprParser.AddContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);
            if (context.op.Text.Equals("+"))
            {
                return left + right;
            }
            else
            {
                return left - right;
            }
        }
        public override int VisitMul([NotNull] PLC_Lab7_exprParser.MulContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);
            if (context.op.Text.Equals("*"))
            {
                return left * right;
            }
            else
            {
                return left / right;
            }
        }
        public override int VisitProg([NotNull] PLC_Lab7_exprParser.ProgContext context)
        {
            foreach (var expr in context.expr())
            {
                Console.WriteLine(Visit(expr));
            }
            return 0;
        }
        */
    }
}
