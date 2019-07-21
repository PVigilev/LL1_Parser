using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    interface ILexer<T> where T : IToken
    {
        IList<T> Tokenize(string str);
    }
}
