using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Lab7
{
    public class EvalVisitor : PLC_Lab7_exprBaseVisitor<(Type Type, string Instruction)>
    {
        SymbolTable symbolTable = new SymbolTable();
        private int available_label = -1;

        private int GetLabel()
        {
            available_label++;
            return available_label;
        }

        public override (Type Type, string Instruction) VisitInt([NotNull] PLC_Lab7_exprParser.IntContext context)
        {
            return (Type.INT, $"PUSH I {int.Parse(context.INT().GetText())}\n");
        }
        public override (Type Type, string Instruction) VisitFloat([NotNull] PLC_Lab7_exprParser.FloatContext context)
        {
            return (Type.FLOAT, $"PUSH F {float.Parse(context.FLOAT().GetText())}\n");
        }
        public override (Type Type, string Instruction) VisitBool([NotNull] PLC_Lab7_exprParser.BoolContext context)
        {
            return (Type.BOOL, $"PUSH B {bool.Parse(context.BOOL().GetText())}\n");
        }
        public override (Type Type, string Instruction) VisitString([NotNull] PLC_Lab7_exprParser.StringContext context)
        {
            return (Type.STRING, $"PUSH S {context.STRING().GetText()}\n");
        }
        public override (Type Type, string Instruction) VisitId([NotNull] PLC_Lab7_exprParser.IdContext context)
        {
            if (context.ID().Symbol.Text == "true")
                return (Type.BOOL, "PUSH B TRUE\n");
            if (context.ID().Symbol.Text == "false")
                return (Type.BOOL, "PUSH B FALSE\n");

            var tmp = symbolTable[context.ID().Symbol];
            return (tmp.Type, $"LOAD {context.ID().Symbol.Text}\n");
        }
        public override (Type Type, string Instruction) VisitType_kw([NotNull] PLC_Lab7_exprParser.Type_kwContext context)
        {
            if (context.type.Text.Equals("int")) return (Type.INT, "0");
            else if (context.type.Text.Equals("float")) return (Type.FLOAT, "0");
            else if (context.type.Text.Equals("bool")) return (Type.BOOL, "FALSE");
            else if (context.type.Text.Equals("string")) return (Type.STRING, "\"\"");
            else return (Type.ERROR, "");
        }
        public override (Type Type, string Instruction) VisitProg([NotNull] PLC_Lab7_exprParser.ProgContext context)
        {
            var sb = new StringBuilder();
            foreach (var statement in context.statement())
            {
                var tmp = Visit(statement);
                sb.Append(tmp.Instruction);
            }
            return (Type.ERROR, sb.ToString());
        }
        public override (Type Type, string Instruction) VisitEmptyStatement([NotNull] PLC_Lab7_exprParser.EmptyStatementContext context)
        {
            return (Type.ERROR, "");
        }

        public override (Type Type, string Instruction) VisitDeclaration([NotNull] PLC_Lab7_exprParser.DeclarationContext context)
        {
            StringBuilder sb = new StringBuilder();
            var type = Visit(context.type_kw());
            foreach (var id in context.ID())
            {
                symbolTable.Add(id.Symbol, type.Type);
                sb.AppendLine($"PUSH {type.Type.ToString()[0]} {type.Instruction}");
                sb.AppendLine($"SAVE {id.Symbol.Text}");
            }

            return (Type.ERROR, sb.ToString());
        }
        public override (Type Type, string Instruction) VisitExpression([NotNull] PLC_Lab7_exprParser.ExpressionContext context)
        {
            var result = Visit(context.expr());

            return (Type.ERROR, result.Instruction + "POP\n");
        }
        public override (Type Type, string Instruction) VisitReadCLI([NotNull] PLC_Lab7_exprParser.ReadCLIContext context)
        {
            var sb = new StringBuilder();
            foreach (var id in context.ID())
            {
                var tmp = symbolTable[id.Symbol];
                sb.AppendLine($"READ {tmp.Type.ToString()[0]}");
                sb.AppendLine($"SAVE {id.Symbol.Text}");
            }
            return (Type.ERROR, sb.ToString());
        }
        public override (Type Type, string Instruction) VisitWriteCLI([NotNull] PLC_Lab7_exprParser.WriteCLIContext context)
        {
            var sb = new StringBuilder();
            int count = 0;

            foreach (var expr in context.expr())
            {
                var tmp = Visit(expr);
                sb.Append(tmp.Instruction);
                count++;
            }
            sb.AppendLine($"PRINT {count}");
            return (Type.ERROR, sb.ToString());
        }
        public override (Type Type, string Instruction) VisitCodeBlock([NotNull] PLC_Lab7_exprParser.CodeBlockContext context)
        {
            var sb = new StringBuilder();
            foreach (var statement in context.statement())
            {
                var tmp = Visit(statement);
                sb.Append(tmp.Instruction);
            }
            return (Type.ERROR, sb.ToString());
        }
        public override (Type Type, string Instruction) VisitIfStatement([NotNull] PLC_Lab7_exprParser.IfStatementContext context)
        {
            var sb = new StringBuilder();
            var cond = Visit(context.expr());
            if (cond.Type != Type.BOOL)
            {
                Errors.ReportError(context.expr().Start, $"Condition {context.expr().GetText()} must be bool. Instead is {cond.Type.ToString()}");
                return (Type.ERROR, "");
            }
            int index = 0;
            (Type Type, string Instruction) if_code = (Type.ERROR, "");
            (Type Type, string Instruction) else_code = (Type.ERROR, "");
            foreach (var statement in context.statement())
            {
                Visit(statement);
                if (index == 0)
                {
                    if_code = Visit(statement);
                }
                else if (index == 1)
                {
                    else_code = Visit(statement);
                }
                index++;
            }

            int triggered_label = GetLabel();
            int continue_label = GetLabel();
            int else_triggered_label = GetLabel();
            int question_label = GetLabel();

            sb.Append(cond.Instruction);
            sb.AppendLine($"JMP {question_label}");
            sb.AppendLine($"LABEL {triggered_label}");
            sb.AppendLine("POP");
            sb.Append(if_code.Instruction);
            sb.AppendLine($"JMP {continue_label}");
            sb.AppendLine($"LABEL {question_label}");
            sb.AppendLine($"FJMP {else_triggered_label}");
            sb.AppendLine($"JMP {triggered_label}");
            sb.AppendLine($"LABEL {else_triggered_label}");
            sb.AppendLine("POP");
            if (index == 2)
            {
                sb.Append(else_code.Instruction);
            }
            sb.AppendLine($"LABEL {continue_label}");
            return (Type.ERROR, sb.ToString());
        }
        public override (Type Type, string Instruction) VisitWhileStatement([NotNull] PLC_Lab7_exprParser.WhileStatementContext context)
        {
            var sb = new StringBuilder();
            var cond = Visit(context.expr());
            if (cond.Type != Type.BOOL)
            {
                Errors.ReportError(context.expr().Start, $"Condition {context.expr().GetText()} must be bool. Instead is {cond.Type.ToString()}");
                return (Type.ERROR, "");
            }
            var repeating_code = Visit(context.statement());

            var triggered_label = GetLabel();
            var question_label = GetLabel();
            var continue_label = GetLabel();
            var pre_condition_label = GetLabel();

            sb.AppendLine($"LABEL {pre_condition_label}");
            sb.Append(cond.Instruction);
            sb.AppendLine($"JMP {question_label}");
            sb.AppendLine($"LABEL {triggered_label}");
            sb.AppendLine("POP");
            sb.Append(repeating_code.Instruction);
            sb.AppendLine($"JMP {pre_condition_label}");
            sb.AppendLine($"LABEL {question_label}");
            sb.AppendLine($"FJMP {continue_label}");
            sb.AppendLine($"JMP {triggered_label}");
            sb.AppendLine($"LABEL {continue_label}");
            sb.AppendLine("POP");

            return (Type.ERROR, sb.ToString());
        }
        public override (Type Type, string Instruction) VisitUnaryMinus([NotNull] PLC_Lab7_exprParser.UnaryMinusContext context)
        {
            var operand = Visit(context.expr());
            if (operand.Type != Type.INT && operand.Type != Type.FLOAT)
            {
                Errors.ReportError(context.expr().Start, $"Unary minus operand {context.expr().GetText()} must be int or float. Instead is {operand.Type.ToString()}");
                return (Type.ERROR, "");
            }
            return (operand.Type, $"{operand.Instruction}UMINUS\n");
        }
        public override (Type Type, string Instruction) VisitUnaryNeg([NotNull] PLC_Lab7_exprParser.UnaryNegContext context)
        {
            var operand = Visit(context.expr());
            if (operand.Type != Type.BOOL)
            {
                Errors.ReportError(context.expr().Start, $"Unary minus operand {context.expr().GetText()} must be bool. Instead is {operand.Type.ToString()}");
                return (Type.ERROR, "");
            }
            return (Type.BOOL, $"{operand.Instruction}NOT\n");
        }

        public override (Type Type, string Instruction) VisitAssignmentLevel([NotNull] PLC_Lab7_exprParser.AssignmentLevelContext context)
        {
            return Visit(context.assignment_expr());
        }

        public override (Type Type, string Instruction) VisitNestedAss([NotNull] PLC_Lab7_exprParser.NestedAssContext context)
        {
            var lhs = symbolTable[context.ID().Symbol];
            var rhs = Visit(context.assignment_expr());
            var sb = new StringBuilder(rhs.Instruction);
            if (lhs.Type != rhs.Type)
            {
                if (lhs.Type == Type.FLOAT && rhs.Type == Type.INT)
                {
                    sb.AppendLine("ITOF");
                    sb.AppendLine($"SAVE {context.ID().Symbol.Text}");
                    sb.AppendLine($"LOAD {context.ID().Symbol.Text}");
                    return (Type.FLOAT, sb.ToString());
                }
                Errors.ReportError(context.assignment_expr().Start, $"Right side has different type than {context.ID().GetText()}. Left side is {lhs.Type.ToString()}. Right side is {rhs.Type.ToString()}.");
                return (Type.ERROR, "");
            }
            sb.AppendLine($"SAVE {context.ID().Symbol.Text}");
            sb.AppendLine($"LOAD {context.ID().Symbol.Text}");

            return (lhs.Type, sb.ToString());
        }

        public override (Type Type, string Instruction) VisitLeavingAss([NotNull] PLC_Lab7_exprParser.LeavingAssContext context)
        {
            return Visit(context.assignment_leaf());
        }

        public override (Type Type, string Instruction) VisitNestedOr([NotNull] PLC_Lab7_exprParser.NestedOrContext context)
        {
            var lhs = Visit(context.or_expr());
            var rhs = Visit(context.or_leaf());
            var sb = new StringBuilder(lhs.Instruction);
            if (lhs.Type != rhs.Type)
            {
                Errors.ReportError(context.or_expr().Start, $"Right side has different type than left side. Left side is {lhs.Type}. Right side is {rhs.Type}.");
                return (Type.ERROR, "");
            }
            if (lhs.Type != Type.BOOL)
            {
                Errors.ReportError(context.or_expr().Start, $"Both side operands should be bool type. Current left: {lhs.Type}. Current right: {rhs.Type}");
                return (Type.ERROR, "");
            }
            sb.Append(rhs.Instruction);
            sb.AppendLine("OR");
            return (lhs.Type, sb.ToString());
        }

        public override (Type Type, string Instruction) VisitLeavingOr([NotNull] PLC_Lab7_exprParser.LeavingOrContext context)
        {
            return Visit(context.or_leaf());
        }
        public override (Type Type, string Instruction) VisitNestedAnd([NotNull] PLC_Lab7_exprParser.NestedAndContext context)
        {
            var lhs = Visit(context.and_expr());
            var rhs = Visit(context.and_leaf());
            var sb = new StringBuilder(lhs.Instruction);
            if (lhs.Type != rhs.Type)
            {
                Errors.ReportError(context.and_expr().Start, $"Right side has different type than left side. Left side is {lhs.Type.ToString()}. Right side is {rhs.Type.ToString()}.");
                return (Type.ERROR, "");
            }
            if (lhs.Type != Type.BOOL)
            {
                Errors.ReportError(context.and_expr().Start, $"Both side operands should be bool type. Current left: {lhs.Type}. Current right: {rhs.Type}");
                return (Type.ERROR, "");
            }
            sb.Append(rhs.Instruction);
            sb.AppendLine("AND");
            return (lhs.Type, sb.ToString());
        }

        public override (Type Type, string Instruction) VisitLeavingAnd([NotNull] PLC_Lab7_exprParser.LeavingAndContext context)
        {
            return Visit(context.and_leaf());
        }
        public override (Type Type, string Instruction) VisitNestedComp([NotNull] PLC_Lab7_exprParser.NestedCompContext context)
        {
            var lhs = Visit(context.comp_expr());
            var rhs = Visit(context.comp_leaf());
            var sb = new StringBuilder(lhs.Instruction);
            if (lhs.Type != rhs.Type)
            {
                if (lhs.Type == Type.INT && rhs.Type == Type.FLOAT)
                {
                    sb.AppendLine("ITOF");
                    sb.Append(rhs.Instruction);
                    sb.Append(context.op.Type == PLC_Lab7_exprLexer.NOT_EQ_OP ? "EQ\nNOT\n" : "EQ\n");
                    return (Type.BOOL, sb.ToString());
                }
                if (lhs.Type == Type.FLOAT && rhs.Type == Type.INT)
                {
                    sb.Append(rhs.Instruction);
                    sb.AppendLine("ITOF");
                    sb.Append(context.op.Type == PLC_Lab7_exprLexer.NOT_EQ_OP ? "EQ\nNOT\n" : "EQ\n");
                    return (Type.BOOL, sb.ToString());
                }
                Errors.ReportError(context.comp_expr().Start, $"Right side has different type than left side. Left side is {lhs.Type.ToString()}. Right side is {rhs.Type.ToString()}.");
                return (Type.ERROR, "");
            }
            if (lhs.Type == Type.BOOL || lhs.Type == Type.ERROR)
            {
                Errors.ReportError(context.comp_expr().Start, $"Both side operands should be int, float or string type. Current left: {lhs.Type}. Current right: {rhs.Type}");
                return (Type.ERROR, "");
            }

            sb.Append(rhs.Instruction);
            sb.Append(context.op.Type == PLC_Lab7_exprLexer.NOT_EQ_OP ? "EQ\nNOT\n" : "EQ\n");
            return (Type.BOOL, sb.ToString());
        }

        public override (Type Type, string Instruction) VisitLeavingComp([NotNull] PLC_Lab7_exprParser.LeavingCompContext context)
        {
            return Visit(context.comp_leaf());
        }
        public override (Type Type, string Instruction) VisitNestedRel([NotNull] PLC_Lab7_exprParser.NestedRelContext context)
        {
            var lhs = Visit(context.rel_expr());
            var rhs = Visit(context.rel_leaf());
            var sb = new StringBuilder(lhs.Instruction);
            if (lhs.Type != rhs.Type)
            {
                if (lhs.Type == Type.FLOAT && rhs.Type == Type.INT || lhs.Type == Type.INT && rhs.Type == Type.FLOAT )
                {
                    if (lhs.Type == Type.FLOAT)
                    {
                        sb.Append(rhs.Instruction);
                        sb.AppendLine("ITOF");
                        sb.AppendLine(context.op.Type == PLC_Lab7_exprLexer.HIGH_OP ? "GT" : "LT");
                        return (Type.BOOL, sb.ToString());
                    }
                    else
                    {
                        sb.AppendLine("ITOF");
                        sb.Append(rhs.Instruction);
                        sb.AppendLine(context.op.Type == PLC_Lab7_exprLexer.HIGH_OP ? "GT" : "LT");
                        return (Type.BOOL, sb.ToString());
                    }
                }
                Errors.ReportError(context.rel_expr().Start, $"Right side has different type than left side. Left side is {lhs.Type.ToString()}. Right side is {rhs.Type.ToString()}.");
                return (Type.ERROR, "");
            }
            if (lhs.Type != Type.INT && lhs.Type != Type.FLOAT)
            {
                Errors.ReportError(context.rel_expr().Start, $"Both side operands should be int or float. Current left: {lhs.Type}. Current right: {rhs.Type}");
                return (Type.ERROR, "");
            }
            sb.Append(rhs.Instruction);
            sb.AppendLine(context.op.Type == PLC_Lab7_exprLexer.HIGH_OP ? "GT" : "LT");
            return (Type.BOOL, sb.ToString());
        }

        public override (Type Type, string Instruction) VisitLeavingRel([NotNull] PLC_Lab7_exprParser.LeavingRelContext context)
        {
            return Visit(context.rel_leaf());
        }
        public override (Type Type, string Instruction) VisitNestedAdd([NotNull] PLC_Lab7_exprParser.NestedAddContext context)
        {
            var lhs = Visit(context.add_expr());
            var rhs = Visit(context.add_leaf());
            var sb = new StringBuilder();
            if (context.op.Type == PLC_Lab7_exprParser.ADD_OP || context.op.Type == PLC_Lab7_exprParser.MIN_OP)
            {
                sb.Append(lhs.Instruction);
                if (lhs.Type == rhs.Type)
                {
                    if (lhs.Type == Type.INT)
                    {
                        sb.Append(rhs.Instruction);
                        sb.AppendLine(context.op.Type == PLC_Lab7_exprParser.ADD_OP ? "ADD" : "SUB");
                        return (lhs.Type, sb.ToString());
                    }
                    if (lhs.Type == Type.FLOAT)
                    {
                        sb.Append(rhs.Instruction);
                        sb.AppendLine(context.op.Type == PLC_Lab7_exprParser.ADD_OP ? "ADD" : "SUB");
                        return (lhs.Type, sb.ToString());
                    }
                    return (Type.ERROR, "");
                }
                if (lhs.Type == Type.INT && rhs.Type == Type.FLOAT)
                {
                    sb.AppendLine("ITOF");
                    sb.Append(rhs.Instruction);
                    sb.AppendLine(context.op.Type == PLC_Lab7_exprParser.ADD_OP ? "ADD" : "SUB");
                    return (rhs.Type, sb.ToString());
                }
                if (lhs.Type == Type.FLOAT && rhs.Type == Type.INT)
                {
                    sb.Append(rhs.Instruction);
                    sb.AppendLine("ITOF");
                    sb.AppendLine(context.op.Type == PLC_Lab7_exprParser.ADD_OP ? "ADD" : "SUB");
                    return (lhs.Type, sb.ToString());
                }
                Errors.ReportError(context.add_expr().Start, $"Both + or - operators should be either INT or FLOAT type. Left is: {lhs.Type}. Right is: {rhs.Type}");
                return (Type.ERROR, "");
            }
            else if (context.op.Type == PLC_Lab7_exprParser.CONCAT_OP)
            {
                if (lhs.Type == rhs.Type && lhs.Type == Type.STRING)
                {
                    sb.Append(lhs.Instruction);
                    sb.Append(rhs.Instruction);
                    sb.AppendLine("CONCAT");
                    return (Type.STRING, sb.ToString());
                }
                else if (lhs.Type == Type.INT && rhs.Type == Type.INT)
                {
                    sb.Append($"PUSH F {lhs.Instruction.Substring(7, 1)}.{rhs.Instruction.Substring(7, 1)}\n");
                    return (Type.FLOAT, sb.ToString());
                }
                Errors.ReportError(context.add_expr().Start, $"Both concatenation operands should be of type string. Left: {lhs.Type.ToString()}. Right: {rhs.Type.ToString()} ");
                return (Type.ERROR, "");
            }
            return (Type.ERROR, "");
        }

        public override (Type Type, string Instruction) VisitLeavingAdd([NotNull] PLC_Lab7_exprParser.LeavingAddContext context)
        {
            return Visit(context.add_leaf());
        }
        public override (Type Type, string Instruction) VisitNestedMul([NotNull] PLC_Lab7_exprParser.NestedMulContext context)
        {
            var lhs = Visit(context.mul_expr());
            var rhs = Visit(context.mul_leaf());
            var sb = new StringBuilder(lhs.Instruction);
            
            if (context.op.Type == PLC_Lab7_exprParser.MUL_OP || context.op.Type == PLC_Lab7_exprParser.DIV_OP )
            {
                if (lhs.Type == rhs.Type)
                {
                    if (lhs.Type == Type.INT)
                    {
                        sb.Append(rhs.Instruction);
                        sb.AppendLine(context.op.Type == PLC_Lab7_exprParser.MUL_OP ? "MUL" : "DIV");
                        return (lhs.Type, sb.ToString());
                    }
                    if (lhs.Type == Type.FLOAT)
                    {
                        sb.Append(rhs.Instruction);
                        sb.AppendLine(context.op.Type == PLC_Lab7_exprParser.MUL_OP ? "MUL" : "DIV");
                        return (lhs.Type, sb.ToString());
                    }
                }
                if (lhs.Type == Type.INT && rhs.Type == Type.FLOAT)
                {
                    sb.AppendLine("ITOF");
                    sb.Append(rhs.Instruction);
                    sb.AppendLine(context.op.Type == PLC_Lab7_exprParser.MUL_OP ? "MUL" : "DIV");
                    return (rhs.Type, sb.ToString());
                }
                if (lhs.Type == Type.FLOAT && rhs.Type == Type.INT)
                {
                    sb.Append(rhs.Instruction);
                    sb.AppendLine("ITOF");
                    sb.AppendLine(context.op.Type == PLC_Lab7_exprParser.MUL_OP ? "MUL" : "DIV");
                    return (lhs.Type, sb.ToString());
                }
                Errors.ReportError(context.mul_expr().Start, $"Both * or / operators should be either INT or FLOAT type. Left is: {lhs.Type}. Right is: {rhs.Type}");
                return (Type.ERROR, "");
            }
            else if (context.op.Type == PLC_Lab7_exprParser.MOD_OP) 
            {
                if (lhs.Type == rhs.Type && lhs.Type == Type.INT)
                {
                    sb.Append(rhs.Instruction);
                    sb.AppendLine("MOD");
                    return (lhs.Type, sb.ToString());
                }
                Errors.ReportError(context.mul_expr().Start, $"Modulo operators should be INT type. Left is: {lhs.Type}. Right is: {rhs.Type}");
                return (Type.ERROR, "");
            }
            return (Type.ERROR, "");
        }

        public override (Type Type, string Instruction) VisitLeavingMul([NotNull] PLC_Lab7_exprParser.LeavingMulContext context)
        {
            return Visit(context.mul_leaf());
        }

        public override (Type Type, string Instruction) VisitCommonUnnary([NotNull] PLC_Lab7_exprParser.CommonUnnaryContext context)
        {
            return Visit(context.unnary_expr());
        }

        public override (Type Type, string Instruction) VisitCommonParent([NotNull] PLC_Lab7_exprParser.CommonParentContext context)
        {
            return Visit(context.expr());
        }
        public override (Type Type, string Instruction) VisitCommonLeaf([NotNull] PLC_Lab7_exprParser.CommonLeafContext context)
        {
            return Visit(context.leaf());
        }
        public override (Type Type, string Instruction) VisitMulTerminal([NotNull] PLC_Lab7_exprParser.MulTerminalContext context)
        {
            return Visit(context.leaf_common());
        }

        public override (Type Type, string Instruction) VisitAddTerminal([NotNull] PLC_Lab7_exprParser.AddTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, string Instruction) VisitAddToMul([NotNull] PLC_Lab7_exprParser.AddToMulContext context)
        {
            return Visit(context.mul_expr());
        }
        public override (Type Type, string Instruction) VisitRelTerminal([NotNull] PLC_Lab7_exprParser.RelTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, string Instruction) VisitRelToAdd([NotNull] PLC_Lab7_exprParser.RelToAddContext context)
        {
            return Visit(context.add_expr());
        }
        public override (Type Type, string Instruction) VisitCompTerminal([NotNull] PLC_Lab7_exprParser.CompTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, string Instruction) VisitCompToRel([NotNull] PLC_Lab7_exprParser.CompToRelContext context)
        {
            return Visit(context.rel_expr());
        }
        public override (Type Type, string Instruction) VisitAndTerminal([NotNull] PLC_Lab7_exprParser.AndTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, string Instruction) VisitAndToComp([NotNull] PLC_Lab7_exprParser.AndToCompContext context)
        {
            return Visit(context.comp_expr());
        }
        public override (Type Type, string Instruction) VisitOrTerminal([NotNull] PLC_Lab7_exprParser.OrTerminalContext context)
        {
            return Visit(context.leaf_common());
        }

        public override (Type Type, string Instruction) VisitOrToAnd([NotNull] PLC_Lab7_exprParser.OrToAndContext context)
        {
            return Visit(context.and_expr());
        }
        public override (Type Type, string Instruction) VisitAssTerminal([NotNull] PLC_Lab7_exprParser.AssTerminalContext context)
        {
            return Visit(context.leaf_common());
        }
        public override (Type Type, string Instruction) VisitAssToOr([NotNull] PLC_Lab7_exprParser.AssToOrContext context)
        {
            return Visit(context.or_expr());
        }
    }
}
