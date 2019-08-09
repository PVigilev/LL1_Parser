using System;
using System.Collections.Generic;
using System.Text;

namespace MFFParser
{
#if DEBUG
    public
#endif
    abstract class AbstractToken : IToken
    {
        public abstract bool IsCompatible(IToken other);
    }
#if DEBUG
    public
#endif
    class SimpleToken : AbstractToken
    {
        public static readonly Dictionary<TokenType, SimpleToken> Instances = new Dictionary<TokenType, SimpleToken>();
        static SimpleToken()
        {
            for (int i = 0; i <= 7; i++)
                Instances.Add((TokenType)i, new SimpleToken((TokenType)i));
        }
        private SimpleToken(TokenType type)
            => Type = type;
        public enum TokenType { column, semicolumn, comma, OB, CB, OCB, CCB, dot}
        public TokenType Type { get; }
        
        public override bool IsCompatible(IToken other)
        {
            if (other.GetType() == typeof(SimpleToken))
                return Type == (other as SimpleToken).Type;
            return false;
        }
    }

#if DEBUG
    public
#endif
    class TokenValue<T> : AbstractToken, ITokenWithValue<T>  where T : class
    {
        public TokenValue(T val)
        {
            Value = val;
        }
        public T Value { get; }

        public override bool IsCompatible(IToken other)
        {
            return other.GetType() == this.GetType();
        }
    }
#if DEBUG
    public
#endif
    class TokenSymbol : TokenValue<string>
    {
        public TokenSymbol(string val) : base(val)
        {
        }
    }
#if DEBUG
    public
#endif
    class TokenInt : TokenValue<object>
    {
        public TokenInt(int val) : base(val)
        {
        }
    }
#if DEBUG
    public
#endif
    class TokenDouble : TokenValue<object>
    {
        public TokenDouble(double val) : base(val)
        {
        }
    }
#if DEBUG
    public
#endif
    class TokenString : TokenValue<string>
    {
        public TokenString(string val) : base(val)
        {
        }
    }

#if DEBUG
    public
#endif
    class TokenVarId : TokenValue<object>
    {
        public TokenVarId(uint val) : base(val)
        {
        }
    }
    
#if DEBUG
    public
#endif
    class TokenKeyWord : AbstractToken
    {
        public enum KeyWordType { newKW, staticKW, }
        KeyWordType Type;
        public static readonly Dictionary<KeyWordType, TokenKeyWord> Instances;
        static TokenKeyWord()
        {
            Instances = new Dictionary<KeyWordType, TokenKeyWord>();
            for (int i = 0; i <= 1; i++)
                Instances.Add((KeyWordType)i, new TokenKeyWord((KeyWordType)i));
        }
        private TokenKeyWord(KeyWordType t) { Type = t; }
        public override bool IsCompatible(IToken other)
        {
            if (other.GetType() == typeof(TokenKeyWord))
                return Type == (other as TokenKeyWord).Type;
            return false;
        }
    }
}
