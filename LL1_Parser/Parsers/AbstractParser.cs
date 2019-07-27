using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{

    public abstract class AbstractParser<Token> where Token : IToken
    {
        public class NotParsedObject
        {
            private NotParsedObject() { }
            public static readonly NotParsedObject Instance = new NotParsedObject();
        }
        protected Grammar Grammar;
        public abstract object Parse(IList<Token> tokens);
    }
}
