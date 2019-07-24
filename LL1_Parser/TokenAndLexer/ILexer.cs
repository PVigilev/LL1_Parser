using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    public interface ILexer<T> where T : IToken
    {
        IList<T> Tokenize(string str);
    }
}
