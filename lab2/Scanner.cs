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
        private char current;

        public Scanner(StreamReader file) 
        { 
            input = file.ReadToEnd();
            position = -1;
            nextChar(true);
        
        }

        public Token NextToken()
        {
            if (current == '\0')
            {
                return new Token(TokenType.EOF, "");
            }

            if (current == '(') 
            {
                nextChar(true);
                return new Token(TokenType.LPAR, "");
            }
            if (current == ')')
            {
                nextChar(true);
                return new Token(TokenType.RPAR, "");
            }
            if (current == ';')
            {
                nextChar(true);
                return new Token(TokenType.DEL, "");
            }
            if (current == '+')
            {
                nextChar(true);
                return new Token(TokenType.OP, "+");
            }
            if (current == '-')
            {
                nextChar(true);
                return new Token(TokenType.OP, "-");
            }
            if (current == '*')
            {
                nextChar(true);
                return new Token(TokenType.OP, "*");
            }

            if(char.IsDigit(current)) 
            {
                string text = "";
                while(char.IsDigit(current)) 
                {
                    text += current;
                    nextChar(true);
                }
                return new Token(TokenType.NUM, text);
            }

            if (char.IsLetter(current))
            {
                string text = "";
                
                while (char.IsLetterOrDigit(current))
                {
                    text += current;
                    nextChar(true);
                    if (text == "mod")
                    {
                        return new Token(TokenType.MOD, "");
                    }
                    if (text == "div")
                    {
                        return new Token(TokenType.DIV, "");
                    }
                }

                return new Token(TokenType.ID, text);
                
            }
            if (current == '/')
            {
                nextChar(true);
                if (current == '/')
                {
                    while(current != '\n')
                    {
                        nextChar(false);
                    }
                    nextChar(true);
                    return new Token(TokenType.NOTE, "");
                }
                else
                {
                    return new Token(TokenType.OP, "/");
                }
                
            }
            else
            {
                return new Token(TokenType.INVALID, "");
            }

        }

        private void nextChar(bool ignoreWhiteSpace)
        {
            position++;
            if (position == input.Length)
                current = '\0';       
            else
                current = input[position];
            if (ignoreWhiteSpace)
            {
                if (char.IsWhiteSpace(current))
                {
                    nextChar(true);
                }
            }

        }
    }
}
