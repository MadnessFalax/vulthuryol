using Grammar;
using System.Collections.Generic;

namespace Lab3
{
	public class GrammarOps
	{
		public GrammarOps(IGrammar g)
		{
			this.g = g;
			compute_empty();
			compute_first();
			compute_follow();
		}

		public ISet<Nonterminal> EmptyNonterminals { get; } = new HashSet<Nonterminal>();

		private void compute_empty()
		{
			///TODO: Add your code here...

			var repeat = true;

			// find default empties
			foreach(var rule in g.Rules)
			{
				if (rule.RHS.Count == 0)
				{
					EmptyNonterminals.Add(rule.LHS);
					repeat = true;
				}
			}

			// handle substitutions
			while(repeat)
			{
				repeat = false;
				foreach (var rule in g.Rules)
				{
					var counter = 0;

					foreach (var symbol in rule.RHS)
					{
						if (symbol is Nonterminal s)
						{
							if (EmptyNonterminals.Contains(s))
							{
								counter++;
							}
						}
					}
					if (counter == rule.RHS.Count)
					{
						if (!EmptyNonterminals.Contains(rule.LHS))
						{
							EmptyNonterminals.Add(rule.LHS);
							repeat = true;
						}
					}
				}
			}
		}

		private void compute_first()
		{
			bool[,] table = new bool[g.Nonterminals.Count, g.Nonterminals.Count + g.Terminals.Count];

			// compute initial table
			foreach(var rule in g.Rules)
			{
				var row_index = g.Nonterminals.IndexOf(rule.LHS);
				var col_index = -1;

				var cur_index = 0;

				while (cur_index != rule.RHS.Count)
				{
					var cur = rule.RHS[cur_index];
					cur_index++;

					if (cur is Terminal t)
					{
						col_index = g.Nonterminals.Count + g.Terminals.IndexOf(t);
						table[row_index, col_index] = true;
						// dev
						//Console.WriteLine($"Adding {t.Name} to {row_index}, {col_index}");
						break;
					}
					
					if (cur is Nonterminal nt)
					{
						col_index = g.Nonterminals.IndexOf(nt);
						table[row_index, col_index] = true;
                        // dev
						//Console.WriteLine($"Adding {nt.Name} to {row_index}, {col_index}");

                        if (!EmptyNonterminals.Contains(nt))
						{
							break;
						}
						// dev
						//Console.WriteLine("is in empty triggered");
					}
				}
			}

			var repeat = true;
			// now add corresponding rows together
			while(repeat)
			{
				repeat = false;
				
				for (var row = 0; row < g.Nonterminals.Count; row++)
				{
					for (var col = 0; col < g.Nonterminals.Count; col++)
					{
						if (row != col && table[row, col])
						{
							Console.WriteLine("Addition triggered!");
							for (var index = g.Nonterminals.Count; index < g.Nonterminals.Count + g.Terminals.Count; index++)
							{
								if (table[col, index])
								{
									if (table[col, index] != table[row, index])
										repeat = true;
									table[row, index] = true;
								}
							}
						}
					}
				}
			}

			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					Console.WriteLine(table[i,j].ToString());
				}
			}

			// result
			foreach(var rule in g.Rules)
			{
				if (rule.RHS.Count != 0)
				{
					var index = 0;
					while (index < rule.RHS.Count)
					{
						if (rule.RHS[index] is Terminal)
						{
							if (!rule.First.Contains(rule.RHS[index]))
								rule.First.Add(rule.RHS[index]);
							break;
						}
						else if (rule.RHS[index] is Nonterminal nt) 
						{
							var row = g.Nonterminals.IndexOf(nt);
							for (var col = g.Nonterminals.Count; col < g.Nonterminals.Count + g.Terminals.Count; col++)
							{
								if (table[row, col])
								{
									if (!rule.First.Contains(g.Terminals[col - g.Nonterminals.Count]))
										rule.First.Add(g.Terminals[col - g.Nonterminals.Count]);
								}
							}
							if (EmptyNonterminals.Contains(rule.RHS[index]))
							{
								index++;

							}
						}
					}
					if (rule.RHS.All(x => EmptyNonterminals.Contains(x)))
					{
						rule.First.Add(new Terminal("{e}"));
					}
				}
				else
				{
					rule.First.Add(new Terminal("{e}"));
				}

			}
		}

		public void PrintFirst()
		{
			foreach (var rule in g.Rules)
			{
				Console.WriteLine($"first[{rule.LHS.Name}:{rule.RHS_toString()}] = {rule.First_toString()}");
			}
		}

		private void compute_follow()
		{
			foreach(var nonterminal in g.Nonterminals)
			{
				
			}
		}

		public void PrintFollow()
		{

		}


		private IGrammar g;
	}
}
