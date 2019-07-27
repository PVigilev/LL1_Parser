﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    /// <summary>
    /// The abstract class for a representation of a simgle symbol of the rule in a grammar
    /// </summary>
    public abstract class Symbol { }

    /// <summary>
    /// Terminal is represented by a token and a function that van resolve if some token is comaptible
    /// with the token that this terminal is represented by.
    /// </summary>
    public class Terminal : Symbol
    {
        public static readonly Terminal EmptyString = new Terminal();
        private Terminal() { }
        public Terminal(IToken Compatible)
        {
            if (Compatible == null)
                throw new ArgumentNullException("Compatible token is null");
            CompatibleToken = Compatible;
        }
        public IToken CompatibleToken { get; }
        public bool IsCompatiable(IToken token) {
            if (token == null)
                throw new ArgumentNullException();
            return token.Equals(CompatibleToken);
        }
    }

#if DEBUG
    public
#endif
    class NonTerminal : Symbol { }
}
