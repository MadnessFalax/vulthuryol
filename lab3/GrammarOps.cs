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
			CheckLL1();
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
							// dev
							//Console.WriteLine("Addition triggered!");
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

			// result
			foreach(var rule in g.Rules)
			{
				if (rule.RHS.Count != 0)
				{
					var index = 0;
					while (index < rule.RHS.Count)
					{
						if (rule.RHS[index] is Terminal ter)
						{
							if (!rule.First.Contains(ter))
								rule.First.Add(ter);
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
						rule.First.Add(g.Epsilon);
					}
				}
				else
				{
					rule.First.Add(g.Epsilon);
				}

			}

			foreach (var rule in g.Rules)
			{
				foreach (var first in rule.First)
				{
					if (!rule.LHS.First.Contains(first) && first != g.Epsilon)
						rule.LHS.First.Add(first);
				}
			}

			foreach (var rule in g.Rules)
			{
				if (rule.First.Count == 0)
				{
					rule.First.Add(g.Epsilon);
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
			var epsilon_col = g.Nonterminals.Count + g.Terminals.Count;
			var nt_offset = g.Nonterminals.Count;

            bool[,] table = new bool[nt_offset, g.Nonterminals.Count + g.Terminals.Count + 1];


			table[g.Nonterminals.IndexOf(g.StartingNonterminal), epsilon_col] = true;

            foreach (var rule in g.Rules)
			{
				for (int i = 0; i < rule.RHS.Count; i++)
				{
					var current = rule.RHS[i];
					
					if (current is Nonterminal cast_current)
					{
						if (i != rule.RHS.Count - 1)
						{
							int next_index = i + 1;

							while(next_index < rule.RHS.Count)
							{
								var next = rule.RHS[next_index];
								
								if (next is Nonterminal cast_next_nt)
								{
									foreach (var terminal in cast_next_nt.First)
									{
										table[g.Nonterminals.IndexOf(cast_current), g.Terminals.IndexOf(terminal) + nt_offset] = true;
									}

									if (!EmptyNonterminals.Contains(cast_next_nt))
									{
										break;
									}
								}
								else if (next is Terminal cast_next)
								{
									table[g.Nonterminals.IndexOf(cast_current), g.Terminals.IndexOf(cast_next) + nt_offset] = true;
									break;
								}

								next_index++;
							}							
						}
						else
						{
							table[g.Nonterminals.IndexOf(cast_current), g.Nonterminals.IndexOf(rule.LHS)] = true;
						}
					}
				}
			}

			var changed = true;

			while (changed)
			{
				changed = false;

				for (int row = 0; row < nt_offset; row++)
				{
					for (int col = 0; col < nt_offset; col++)
					{
						if (table[row, col] && row != col)
						{
                            for (var index = nt_offset; index < epsilon_col + 1 /* includes epsilon */; index++)
                            {
                                if (table[col, index])
                                {
                                    if (table[col, index] != table[row, index])
                                        changed = true;
                                    table[row, index] = true;
                                }
                            }
                        }
					}
				}
			}

			Terminal endline = new Terminal("$");

			for (int row = 0; row < nt_offset; row++)
			{
				var current_nt = g.Nonterminals[row];

				for (int col = nt_offset; col < epsilon_col; col++)
				{
					var current_t = g.Terminals[col - nt_offset];

					if (table[row, col])
					{
						if (!current_nt.Follow.Contains(current_t))
						{
							current_nt.Follow.Add(current_t);
						}
					}
				}

				if (table[row, epsilon_col])
				{
                    if (!current_nt.Follow.Contains(endline))
                    {
                        current_nt.Follow.Add(endline);
                    }
                }
			}
        }

		public void PrintFollow()
		{
			foreach(var nt in g.Nonterminals)
			{
				Console.WriteLine($"follow[{nt.Name}] = {nt.Follow_toString()}");
			}
		}

		private void CheckLL1()
		{
			IList<Terminal>[] per_nonterminal = new List<Terminal>[g.Nonterminals.Count];
			for(int i = 0; i <  g.Nonterminals.Count; i++)
			{
				per_nonterminal[i] = new List<Terminal>();
			}

			foreach(var rule in g.Rules)
			{
				var row = g.Nonterminals.IndexOf(rule.LHS);
				foreach (Terminal terminal in rule.First)
				{
					if (per_nonterminal[row].Contains(terminal))
					{
						g.LL1 = false;
						Console.WriteLine("Not LL1 due to First-First rule.");
						return;
					}
					else
					{
						per_nonterminal[row].Add(terminal);
					}
				}
			}

			per_nonterminal.Initialize();

			foreach(var rule in g.Rules)
            {
                var row = g.Nonterminals.IndexOf(rule.LHS);
				if (rule.RHS.Contains(g.Epsilon))
				{
                    foreach (Terminal terminal in rule.LHS.Follow)
                    {
                        if (per_nonterminal[row].Contains(terminal))
                        {
                            g.LL1 = false;
							Console.WriteLine("Not LL1 due to First-Follow rule.");
                            return;
                        }
                        else
                        {
                            per_nonterminal[row].Add(terminal);
                        }
                    }
                }
				else
				{
                    foreach (Terminal terminal in rule.First)
                    {
                        if (per_nonterminal[row].Contains(terminal))
                        {
                            g.LL1 = false;
                            Console.WriteLine("Not LL1 due to First-Follow rule.");
                            return;
                        }
                        else
                        {
                            per_nonterminal[row].Add(terminal);
                        }
                    }
                }
            }

			g.LL1 = true;
		}

		public void PrintLL1()
		{
			if (g.LL1)
			{
				Console.WriteLine("It is LL1 grammar");
            }
			else
			{
                Console.WriteLine($"It is not LL1 grammar");
			}
		}


		private IGrammar g;
	}
}
