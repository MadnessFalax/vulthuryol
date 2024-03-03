using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public enum TokenType
    {
        EOF,
        NOTE,
        ID,
        NUM,
        OP,
        DEL,
        DIV,
        MOD,
        LPAR,
        RPAR,
        INVALID
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

        public void printToken()
        {
            if (Type != TokenType.EOF && Type != TokenType.NOTE) 
            {
                Console.WriteLine($"{Type}" + (Value != "" ? $": {Value}" : ""));
            }
        }
    }
}
