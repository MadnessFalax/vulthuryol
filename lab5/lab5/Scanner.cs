using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab5
{

    public class Scanner 
    {
        private readonly string input;
        private int index = -1;
        private char current;
        public bool EOL { get; private set; }
        public Scanner(string input)
        {
            this.input = input;
            NextChar();
        }

        private void NextChar()
        {
            index++;
            if (index >= input.Length)
            {
                EOL = true;
                return;
            }
            current = (char)input[index];
        }
        private void SkipWhiteSpace()
        {
            while (!EOL && char.IsWhiteSpace(current))
                NextChar();
        }

        public Token NextToken()
        {
            if (EOL)
            {
                return  new Token(TokenType.EOL, null);
            }
            SkipWhiteSpace();
            if (current == '/')
            {
                NextChar();
                return new Token(TokenType.DIV, null); 
            }
            if (current == '*')
            {
                NextChar();
                return new Token(TokenType.MUL, null); ;
            }
            if (current == '+')
            {
                NextChar();
                return new Token(TokenType.PLUS, null);
            }
            if (current == '-')
            {
                NextChar();
                return new Token(TokenType.MINUS, null);
            }
            if (current == '(')
            {
                NextChar();
                return new Token(TokenType.LeftPAR, null);
            }
            if (current == ')')
            {
                NextChar();
                return new Token(TokenType.RightPAR, null);
            }


            if (char.IsDigit(current))
            {
                StringBuilder st = new StringBuilder();

                while (!EOL && char.IsDigit(current))
                {
                    st.Append(current);
                    NextChar();
                }
                return new Token(TokenType.NUMBER, st.ToString());
            }
            NextChar();
            return new Token(TokenType.ERROR, "" + current);
        }
    }
}


