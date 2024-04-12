using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Lab7
{
    internal class SymbolTable
    {
        Dictionary<string, (Type Type, string Value)> memory = new Dictionary<string, (Type Type, string Value)>();

        public void Add(IToken variable, Type type)
        {
            var name = variable.Text.Trim();
            if (memory.ContainsKey(name))
            {
                Errors.ReportError(variable, $"Variable {name} was already declared.");
            }
            else
            {
                if (type == Type.INT) memory.Add(name, (type, "0"));
                else if (type == Type.FLOAT) memory.Add(name, (type, "0"));
                else if (type == Type.BOOL) memory.Add(name, (type, "false"));
                else if (type == Type.STRING) memory.Add(name, (type, ""));
                else Errors.ReportError(variable, $"Erroneous type value being added to Symbol Table. Type: {type.ToString()}");
            }
        }
        public void Add(string variable, Type type)
        {
            var name = variable;
            if (memory.ContainsKey(name))
            {
                Errors.ReportError(null, $"Variable {name} was already declared.");
            }
            else
            {
                if (type == Type.INT) memory.Add(name, (type, "0"));
                else if (type == Type.FLOAT) memory.Add(name, (type, "0"));
                else if (type == Type.BOOL) memory.Add(name, (type, "false"));
                else if (type == Type.STRING) memory.Add(name, (type, ""));
                else Errors.ReportError(null, $"Erroneous type value being added to Symbol Table. Type: {type.ToString()}");
            }
        }
        public (Type Type, string Value) this[IToken variable]
        {
            get
            {
                var name = variable.Text.Trim();
                if (memory.ContainsKey(name))
                {
                    return memory[name];
                }
                else
                {
                    Errors.ReportError(variable, $"Variable {name} was NOT declared.");
                    return (Type.ERROR, "");
                }
            }
            set
            {
                var name = variable.Text.Trim();
                memory[name] = value;
            }
        }
        public (Type Type, string Value) this[string variable]
        {
            get
            {
                var name = variable;
                if (memory.ContainsKey(name))
                {
                    return memory[name];
                }
                else
                {
                    Errors.ReportError(null, $"Variable {name} was NOT declared.");
                    return (Type.ERROR, null);
                }
            }
            set
            {
                var name = variable;
                memory[name] = value;
            }
        }
    }
}
