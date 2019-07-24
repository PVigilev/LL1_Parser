using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    public interface IToken : IEquatable<IToken> { }

    public interface ITokenWithValue<out T> : IToken
    {
        T Value { get; }
    }    
}
