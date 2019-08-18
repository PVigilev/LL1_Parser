using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    public interface IToken
    {
        bool IsCompatible(IToken other);
    }

    public interface ITokenWithValue<out T> : IToken where T : class
    {
        T Value { get; }
    }    
}
