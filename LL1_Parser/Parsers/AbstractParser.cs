using System;
using System.Collections.Generic;

namespace LL1_Parser
{
    /// <summary>
    /// Abstract class represents parser-engine
    /// </summary>
    /// <typeparam name="Token">Type of token's data structure</typeparam>
    abstract class AbstractParser<Token> where Token : IToken
    {
        internal class NotParsedObject
        {
            private NotParsedObject() { }
            public static readonly NotParsedObject Instance = new NotParsedObject();
        }

        protected Grammar Grammar;
        internal AbstractParser(Grammar grm)
        {
            Grammar = grm ?? throw new ArgumentNullException("Grammar object is null");
        }

        internal AbstractParser(GrammarCreator gc)
        {
            Grammar = gc.Create();
        }
        public abstract object Parse(IList<Token> tokens);
    }
}
