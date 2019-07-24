using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    public interface IToken : IEquatable<IToken> { }


    // Token, ktery má v sobě nějakou hodnotu
    public interface ITokenWithValue<T> : IToken
    {
        T Value { get; set; }
    }    
}
