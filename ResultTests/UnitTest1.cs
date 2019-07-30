using System;
using System.Collections.Generic;
using LL1_Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ArithmeticsTest
{
    public abstract class Expr
    {
        public abstract int Eval();
    }
    public abstract class BinaryOperation : Expr
    {
        protected Expr Left;
        protected Expr Right;
        public BinaryOperation(Expr left, Expr right)
        {
            Left = left;
            Right = right;
        }
    }
    public class Addition : BinaryOperation
    {
        public Addition(Expr left, Expr right) : base(left, right)
        {
        }

        public override int Eval()
        {
            return Left.Eval() + Right.Eval();
        }
    }

    public class Substract : BinaryOperation
    {
        public Substract(Expr left, Expr right) : base(left, right)
        {
        }

        public override int Eval()
        {
            return Left.Eval() + Right.Eval();
        }
    }
    public class Multiply : BinaryOperation
    {
        public Multiply(Expr left, Expr right) : base(left, right)
        {
        }

        public override int Eval()
        {
            return Left.Eval() + Right.Eval();
        }
    }
    public class Divide : BinaryOperation
    {
        public Divide(Expr left, Expr right) : base(left, right)
        {
        }

        public override int Eval()
        {
            var r = Right.Eval();
            return Left.Eval() / (r == 0 ? throw new DivideByZeroException() : r);
        }
    }
    public class Constant : Expr
    {
        int Value;
        public Constant(int val) { Value = val; }
        public override int Eval()
        {
            return Value;
        }
    }
}


namespace ResultTests
{
    [TestClass]
    public class ParsingApithmeticExprTests
    {
        abstract class AbstractToken : IToken
        {
            public abstract bool IsCompatible(IToken other);
        }
        class Token : AbstractToken
        {
            public enum TokenType { plus, minus, star, slash, ob, cb }
            TokenType Type;
            private Token(TokenType type) { Type = type; }
            public static readonly Dictionary<TokenType, Token> Instances;
            static Token()
            {
                Instances = new Dictionary<TokenType, Token>();
                for(int i = 0; i <= 5; i++)
                {
                    Instances.Add((TokenType)i, new Token((TokenType)i));
                }
            }
            public override bool IsCompatible(IToken other)
            {
                if (other is Token tkn)
                    return Type == tkn.Type;
                return false;
            }
        }
        sealed class TokenInt : AbstractToken, ITokenWithValue<int>
        {
            public int Value { get; }

            public TokenInt(int num) { Value = num; }
            public override bool IsCompatible(IToken other)
            {
                return other.GetType() == typeof(TokenInt);
            }
        }

        class Lexer : ILexer<AbstractToken>
        {
            public Dictionary<string, AbstractToken> NameTokenTBL = new Dictionary<string, AbstractToken>
                {
                    {"+",Token.Instances[Token.TokenType.plus] },
                    {"-",Token.Instances[Token.TokenType.minus] },
                    {"*",Token.Instances[Token.TokenType.star] },
                    {"/",Token.Instances[Token.TokenType.slash] },
                    {"(",Token.Instances[Token.TokenType.ob] },
                    {")",Token.Instances[Token.TokenType.cb] },
                    {"int",new TokenInt(0) }
                };

            public IList<AbstractToken> Tokenize(string str)
            {
                List<AbstractToken> tokens = new List<AbstractToken>();
                HashSet<char> separators = new HashSet<char> { ' ', '+', '-', '/', '*', '(', ')' };
                for(int i = 0; i < str.Length; i++)
                {
                    if (str[i] == '+')
                        tokens.Add(Token.Instances[Token.TokenType.plus]);
                    else if (str[i] == '-')
                        tokens.Add(Token.Instances[Token.TokenType.minus]);
                    else if (str[i] == '*')
                        tokens.Add(Token.Instances[Token.TokenType.star]);
                    else if (str[i] == '/')
                        tokens.Add(Token.Instances[Token.TokenType.slash]);
                    else if (str[i] == '(')
                        tokens.Add(Token.Instances[Token.TokenType.ob]);
                    else if (str[i] == ')')
                        tokens.Add(Token.Instances[Token.TokenType.cb]);
                    else
                    {
                        int j = (str[i] == '-' ? i + 1 : i);
                        bool ok = true;
                        for (; j < str.Length; j++) ;
                        {
                            if (!char.IsDigit(str[j]))
                            {
                                if(!separators.Contains(str[j]))
                                    ok = false;
                                break;
                            }
                        }
                        if (ok)
                        {
                            tokens.Add(IntTokenize(str.Substring(i, j - i)));
                            i = j - 1;
                            continue;
                        }
                        else throw new ParserUnknownTokenException();
                        
                    }
                }
                return tokens;
            }

            TokenInt IntTokenize(string str)
            {
                int res;
                if (!int.TryParse(str, out res))
                    return null;
                return new TokenInt(res);
            }

        }

        [TestMethod]
        public void TestSimpleArithmeticGrammar()
        {
            
            string grammar = null;
            using (StreamReader reader = new StreamReader(@"C:\Users\Paul\Desktop\prg\cs\SemesterProject\LL1_Parser\ResultTests\SimpleArithmeticsGrammar.grm"))
            {
                grammar = reader.ReadToEnd();
            }   
        }
    }
}
