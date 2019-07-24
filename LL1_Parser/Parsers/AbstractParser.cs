using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    abstract class AbstractParser<Token> where Token : IToken
    {
        protected Grammar grammar;
        public abstract object Parse(IList<Token> tokens);
    }
}
