using System.Collections.Generic;
using System.Text;

namespace Grammar
{

	public class Rule
	{

		public Rule(Nonterminal lhs)
		{
			this.LHS = lhs;
		}

		public Nonterminal LHS { get; init; }

		public IList<Symbol> RHS { get; } = new List<Symbol>();

		public IList<Terminal> First { get; } = new List<Terminal>();

		public string RHS_toString()
		{
			StringBuilder sb = new StringBuilder();
			
			foreach (Symbol sym in RHS)
			{
				sb.Append($"{sym.Name}");
			}

			//if (RHS.Count == 0) 
			//{
			//	sb.Append("{e}");
			//}

			return sb.ToString();
		}

        public string First_toString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Symbol sym in First)
            {
                sb.Append($"{sym.Name}");
            }

            return sb.ToString();
        }

    }
}