using System.Collections.Generic;

namespace LL1_Parser.Initialization
{
    /// <summary>
    /// Abstract class for token data structure for initialization
    /// </summary>
    abstract class AbstractToken : IToken
    {
        public abstract bool IsCompatible(IToken other);
    }

    /// <summary>
    /// Simple token representation for initialization
    /// : ; , ( ) { } .
    /// </summary>
    class SimpleToken : AbstractToken
    {
        /// <summary>
        /// Instances of tokens
        /// </summary>
        public static readonly Dictionary<TokenType, SimpleToken> Instances = new Dictionary<TokenType, SimpleToken>();
        static SimpleToken()
        {
            for (int i = 0; i <= 7; i++)
                Instances.Add((TokenType)i, new SimpleToken((TokenType)i));
        }
        private SimpleToken(TokenType type)
            => Type = type;
        public enum TokenType { column, semicolumn, comma, OB, CB, OCB, CCB, dot }
        public TokenType Type { get; }

        public override bool IsCompatible(IToken other)
        {
            if (other.GetType() == typeof(SimpleToken))
                return Type == (other as SimpleToken).Type;
            return false;
        }
    }


    /// <summary>
    /// Token with value implementation for initialization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class TokenValue<T> : AbstractToken, ITokenWithValue<T> where T : class
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

    /// <summary>
    /// keeps name of some symbols name in grammar
    /// </summary>
    class TokenSymbol : TokenValue<string>
    {
        public TokenSymbol(string val) : base(val)
        {
        }
    }

    /// <summary>
    /// keeps some int-value
    /// </summary>
    class TokenInt : TokenValue<object>
    {
        public TokenInt(int val) : base(val)
        {
        }
    }


    /// <summary>
    /// keeps double-value
    /// </summary>
    class TokenDouble : TokenValue<object>
    {
        public TokenDouble(double val) : base(val)
        {
        }
    }

    /// <summary>
    /// keeps string
    /// </summary>
    class TokenString : TokenValue<string>
    {
        public TokenString(string val) : base(val)
        {
        }
    }

    /// <summary>
    /// keeps id of parsing-result in some rule
    /// </summary>
    class TokenVarId : TokenValue<object>
    {
        public TokenVarId(uint val) : base(val)
        {
        }
    }


    /// <summary>
    /// keeps id of some key-word (new, static)
    /// </summary>
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
