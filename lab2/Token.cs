using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public enum TokenType
    {
        EOF,
        ID,
        NUM,
        OP,
        DEL,
        DIV,
        MOD,
        LPAR,
        RPAR
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;    
            Value = value;  
        }
    }
}
