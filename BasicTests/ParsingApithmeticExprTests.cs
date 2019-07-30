using System.Collections.Generic;
using LL1_Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ResultTests
{
    using ArithmeticsTest;
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
                for (int i = 0; i <= 5; i++)
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
                for (int i = 0; i < str.Length; i++)
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
                                if (!separators.Contains(str[j]))
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
            string ExprGrammar = null;
            using (StreamReader reader = new StreamReader(@"C:\Users\Paul\Desktop\prg\cs\SemesterProject\LL1_Parser\BasicTests\SimpleArithmeticsGrammar.grm"))
            {
                ExprGrammar = reader.ReadToEnd();
            }
            if (ExprGrammar == null)
                Assert.Fail("Problem with reading from .grm-file");
            Lexer lexer = new Lexer();
            LL1GrammarParser<Expr, AbstractToken> parser
                = new LL1GrammarParser<Expr, AbstractToken>(ExprGrammar, lexer, lexer.NameTokenTBL, "Expr", new System.Reflection.Assembly[] { System.Reflection.Assembly.GetExecutingAssembly() });
        }
    }
}
