using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace Grammar
{
    public interface IGrammar
    {
        bool LL1 { get; set; } 
        IList<Nonterminal> Nonterminals { get; }
        IList<Terminal> Terminals { get; }
        IList<Rule> Rules { get; }

        Terminal Epsilon { get; }

        Nonterminal? StartingNonterminal { get;  }

        void dump();
    }
}