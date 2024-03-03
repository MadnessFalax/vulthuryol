using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ConsoleApp1
{
    internal class Scanner
    {
        private string input;
        private int position;

        public Scanner(StreamReader file) 
        { 
            input = file.ReadToEnd();
        
        }

        public Token nextToken()
        {
            if (position >=input.Length)
            {
                return new Token(TokenType.EOF, null);
            }

            char current = input[position];
            if (char.IsWhiteSpace(current))
            {
                position++;
                return nextToken();
            }
            if (current == '(' || current == ')' || current == ';' || current == '+' || current == '-' || current == '*')
            {
                position++;
                if (current == '(') return new Token(TokenType.LPAR, null);
                if (current == ')') return new Token(TokenType.RPAR, null);
                if (current == ';') return new Token(TokenType.DEL, null);
                if (current == '+') return new Token(TokenType.OP, "+");
                if (current == '-') return new Token(TokenType.OP, "-");
                if (current == '*') return new Token(TokenType.OP, "*");

            }
            if(char.IsDigit(current)) 
            {
                string text = "";
                while(char.IsDigit(current)) 
                {
                    text += current;
                    position++;
                }
                return new Token(TokenType.NUM, text);
            }
            if (char.IsLetter(current))
            {
                string text = "";
                while (char.IsLetterOrDigit(current))
                {
                    text += current;
                    position++;
                }
                if (text == "mod")
                {
                    return new Token(TokenType.MOD, null);
                }
                if (text == "div")
                {
                    return new Token(TokenType.DIV, null);
                }
                else
                {
                    return new Token(TokenType.ID, text);
                }
            }
            if (current == '/')
            {
                string text = "";
                while (char.IsLetterOrDigit(current))
                {
                    text += current;
                    position++;
                }
                return new Token(TokenType.ID, text);
            }

        }
    }
}
