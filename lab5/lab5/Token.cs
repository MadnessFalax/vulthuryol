using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab5
{
    public enum TokenType
    {
        ERROR,
        NUMBER,
        PLUS,
        MINUS,
        MUL,
        DIV,
        LeftPAR,
        RightPAR,
        EOL
    }
    public class Token
    {
        public Token(TokenType Type, string Value)
        {
            this.Type = Type;
            this.Value = Value;
        }
        public TokenType Type { get; }
        public string Value { get; }

        public override string ToString()
        {
            return "(" + Type.ToString() + ", " + (string.IsNullOrEmpty(Value) ? "" : Value)+")";
        }
    }
}
