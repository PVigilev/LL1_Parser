using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    interface IToken : IEquatable<IToken> { }


    // Token, ktery má v sobě nějakou hodnotu
    interface ITokenWithValue<T> : IToken
    {
        T Value { get; set; }
    }


    
}
