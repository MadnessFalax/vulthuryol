using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab5
{
    public class Parser
    {
        private Scanner scanner;
        private Token token;
        private ICollection<int> rules = new List<int>();

        public string RulesToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var rule in rules)
            {
                sb.Append($"{rule.ToString()} ");
            }

            return sb.ToString(); 
        }

        public Parser(Scanner scanner)
        {
            this.scanner = scanner;
            this.token = scanner.NextToken();
            E();
        }

        public bool error { get; set; }

        private void Expect(TokenType expectedType)
        {
            if (token.Type == expectedType)
            {
                token = scanner.NextToken();
            }
            else
            {
                error = true;
            }
        }

        private void E()
        {
            rules.Add(1);
            T();
            E1();
        }
        private void E1()
        {
            
            if (token.Type == TokenType.PLUS)
            {
                rules.Add(2);
                Expect(TokenType.PLUS);
                T();
                E1();
            }
            else if (token.Type == TokenType.MINUS)
            {
                rules.Add(3);
                Expect(TokenType.MINUS);
                T();
                E1();
            }
            else if (token.Type == TokenType.EOL || token.Type == TokenType.RightPAR)
            {
                rules.Add(4);
            }
            else
            {
                error = true;
            }
        }

        private void T()
        {
            
            rules.Add(5);
            F();
            token = scanner.NextToken();
            T1();
        }

        private void T1()
        {
            
            if (token.Type == TokenType.MUL)
            {
                rules.Add(6);
                Expect(TokenType.MUL);
                F();
                T1();
            }
            else if (token.Type == TokenType.DIV)
            {
                rules.Add(7);
                Expect(TokenType.DIV);
                F();
                T1();
            }
            else if (token.Type == TokenType.EOL || token.Type == TokenType.RightPAR || token.Type == TokenType.PLUS || token.Type == TokenType.MINUS)
            {
                rules.Add(8);
            }
            else
            {
                error = true;
            }
            
        }

        private void F()
        {
            if (token.Type == TokenType.LeftPAR)
            {
                rules.Add(9);
                Expect(TokenType.LeftPAR);
                E();
                Expect(TokenType.RightPAR);
            }
            else if (token.Type == TokenType.NUMBER)
            {
                var value = int.Parse(token.Value);
                rules.Add(10);
            }
            else
            {
                error = true;
            }

        }
    }
}
