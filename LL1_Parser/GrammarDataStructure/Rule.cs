using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{

    public class Rule : IEnumerable<Symbol>
    {
        Symbol[] Symbols;
        internal RuleAction Action { get; }
        public Rule(Symbol[] symbols, RuleAction action)
        {
            Symbols = symbols;
            Action = action;
        }
        public Symbol this[int id] { get { return Symbols[id]; } }
        public int Length { get { return Symbols.Length; } }

        public IEnumerator<Symbol> GetEnumerator()
        {
            return ((IEnumerable<Symbol>)Symbols).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Symbol>)Symbols).GetEnumerator();
        }
    }
}
