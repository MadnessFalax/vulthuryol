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
        private int result = 0;

        public string RulesToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var rule in rules)
            {
                sb.Append($"{rule.ToString()} ");
            }

            return sb.ToString(); 
        }

        public int Result { get { return result; } }

        public Parser(Scanner scanner)
        {
            this.scanner = scanner;
            this.token = scanner.NextToken();
            result = E_interpret();
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

        private int E_interpret()
        {
            rules.Add(1);
            int multiplications_result = T_interpret();
            return E1_interpret(multiplications_result);
        }
        private int E1_interpret(int LHS)
        {
            int result = LHS;
            int RHS = 0;

            if (token.Type == TokenType.PLUS)
            {
                Expect(TokenType.PLUS);
                RHS = T_interpret();
                return E1_interpret(LHS + RHS);
            }
            else if (token.Type == TokenType.MINUS)
            {
                Expect(TokenType.MINUS);
                RHS = T_interpret();
                return E1_interpret(LHS - RHS);
            }
            else if (token.Type == TokenType.EOL || token.Type == TokenType.RightPAR)
            {
                return result;
            }
            else
            {
                error = true;
                return 0;
            }
        }

        private int T_interpret()
        {
            int tmp = 0;
            tmp = F_interpret();
            token = scanner.NextToken();
            return T1_interpret(tmp);
        }

        private int T1_interpret(int LHS)
        {
            int result = LHS;
            int RHS = 0;
            if (token.Type == TokenType.MUL)
            {
                Expect(TokenType.MUL);
                RHS = F_interpret();
                return T1_interpret(LHS * RHS);
            }
            else if (token.Type == TokenType.DIV)
            {
                Expect(TokenType.DIV);
                RHS = F_interpret();
                if (RHS == 0)
                {
                    error = true;
                    return 0;
                }
                return T1_interpret(LHS * RHS);
            }
            else if (token.Type == TokenType.EOL || token.Type == TokenType.RightPAR || token.Type == TokenType.PLUS || token.Type == TokenType.MINUS)
            {
                return result;
            }
            else
            {
                error = true;
                return 0;
            }

        }

        private int F_interpret()
        {
            int result = 0;

            if (token.Type == TokenType.LeftPAR)
            {
                Expect(TokenType.LeftPAR);
                result = E_interpret();
                Expect(TokenType.RightPAR);
            }
            else if (token.Type == TokenType.NUMBER)
            {
                var value = int.Parse(token.Value);
                result = value;
            }
            else
            {
                error = true;
            }

            return result;

        }
    }
}
