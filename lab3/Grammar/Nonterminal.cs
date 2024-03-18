using System;
using System.Collections.Generic;
using System.Text;

namespace Grammar
{
	public class Nonterminal : Symbol
	{
		public Nonterminal(string name) : base(name)
		{
		}
		public IList<Rule> Rules { get; } = new List<Rule>();
		public void AddRule(Rule rule)
		{
			Rules.Add(rule);
		}

		public IList<Terminal> Follow = new List<Terminal>();

        public IList<Terminal> First = new List<Terminal>();

        public string Follow_toString()
		{
            StringBuilder sb = new StringBuilder();

            foreach (Symbol sym in Follow)
            {
                sb.Append($"{sym.Name}");
            }

            return sb.ToString();
        }
	}
}