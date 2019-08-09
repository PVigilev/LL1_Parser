using System;
using System.Collections.Generic;
using System.Text;

namespace MFFParser
{
    public interface ILexer<T> where T : IToken
    {
        IList<T> Tokenize(string str);
    }
}
