using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LL1_Parser;
using LL1_Parser.Initialization;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BasicDS
{
    public class ArrayN : List<int>
    {        
        public override string ToString()
        {
            if (Count == 0)
                return "";
            StringBuilder result = new StringBuilder();
            string separator = ", ";
            for(int i = 0; i < Count-1; i++)
            {
                result.Append(this[i]);
                result.Append(separator);
            }
            result.Append(this[Count - 1]);
            return result.ToString();
        }
        public new void Reverse() => base.Reverse();
        public new void Add(int val) => base.Add(val);
    }
    public static class IntArithmetics
    {
        public static int Add(int a, int b) => a + b;
        public static int Substract(int a, int b) => a - b;
        public static int Divide(int a, int b) => a / b;
        public static int Multiply(int a, int b) => a * b;
        public abstract class AbstractToken : IToken
        {
            public abstract bool IsCompatible(IToken other);
        }
        public class Token : AbstractToken
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
        public sealed class TokenInt : AbstractToken, ITokenWithValue<object>
        {
            public object Value { get; }

            public TokenInt(int num) { Value = num; }
            public override bool IsCompatible(IToken other)
            {
                return other.GetType() == typeof(TokenInt);
            }
        }

        public class Lexer : ILexer<AbstractToken>
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
                    if (char.IsWhiteSpace(str[i]))
                        continue;
                    else if (str[i] == '+')
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
                        for (; j < str.Length; j++)
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
    }
}



namespace BasicTests
{
    using BasicDS;
    [TestClass]
    public class DefaultTokensTest
    {
        private bool CompatiableTokenSequence(IList<AbstractToken> tokens, IList<AbstractToken> expected)
        {
            if (tokens.Count != expected.Count)
                return false;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (!expected[i].IsCompatible(tokens[i]))
                    return false;
            }
            return true;
        }


        [TestMethod]
        public void DefaultTokenizerTest()
        {
            string str = "\"string name1\" name2 : .)({}'}'{123.32 12343 $123 _name3 new static newstatic " + Environment.NewLine + "static_new -123";
            var expected = new List<AbstractToken>(new AbstractToken[] {
            new TokenString("string name1"), new TokenSymbol("name2"),
            SimpleToken.Instances[SimpleToken.TokenType.column],
            SimpleToken.Instances[SimpleToken.TokenType.dot],
            SimpleToken.Instances[SimpleToken.TokenType.CB], SimpleToken.Instances[SimpleToken.TokenType.OB],
            SimpleToken.Instances[SimpleToken.TokenType.OCB], SimpleToken.Instances[SimpleToken.TokenType.CCB],
            new TokenSymbol("}"),
            SimpleToken.Instances[SimpleToken.TokenType.OCB],
            new TokenDouble(123.32), new TokenInt(12343), new TokenVarId(123),
            new TokenSymbol("_name3"), TokenKeyWord.Instances[TokenKeyWord.KeyWordType.newKW],
            TokenKeyWord.Instances[TokenKeyWord.KeyWordType.staticKW],
            new TokenSymbol("newstatic"), new TokenSymbol("static_new"), new TokenInt(-123)});

            Lexer lexer = new Lexer();
            var tokens = lexer.Tokenize(str);
            if (!CompatiableTokenSequence(tokens, expected))
                Assert.Fail();
        }

        [TestMethod]
        public void LeftRecursionCheckerTest()
        {
            Exception exception = null;
            string filename = @"C:\Users\Paul\Desktop\prg\cs\SemesterProject\LL1_Parser\BasicTests\LeftRecursion.txt";
            string grammar;
            using (StreamReader reader = new StreamReader(filename))
            {
                grammar = reader.ReadToEnd();
            }
            if (grammar == null || grammar == "")
                Assert.Fail();
            var lexer = new IntArithmetics.Lexer();
            try
            {
                var parser = new LL1GrammarParser<int, IntArithmetics.AbstractToken>(
                    grammar, lexer, lexer.NameTokenTBL, new System.Reflection.Assembly[] { typeof(IntArithmetics).Assembly });
                Assert.Fail();
            }
            catch (LeftRecursionGrammarException exc)
            {
                exception = exc;
            }
            if (exception == null || exception.GetType() != typeof(LeftRecursionGrammarException))
                Assert.Fail();
        }
    }


    [TestClass]
    public class SimpleGrammarsTests
    {
        [TestMethod]
        public void ListGrammarTest()
        {
            string filename = @"C:\Users\Paul\Desktop\prg\cs\SemesterProject\LL1_Parser\BasicTests\SequenceGrammar.txt";
            string grammar;
            using (StreamReader reader = new StreamReader(filename))
            {
                grammar = reader.ReadToEnd();
            }
            if (grammar == null || grammar == "")
                Assert.Fail();
            Dictionary<string, AbstractToken> kvp = new Dictionary<string, AbstractToken> { { "int", new TokenInt(default(int)) } };
            var parser = new LL1GrammarParser<ArrayN, AbstractToken>(grammar, new Lexer(), kvp, new System.Reflection.Assembly[] { typeof(ArrayN).Assembly });
            var parsed = parser.Parse("1 2 3");
            Assert.AreEqual("1, 2, 3", parsed.ToString());
            parsed = parser.Parse("");
            Assert.AreEqual("", parsed.ToString());
            parsed = parser.Parse("-123");
            Assert.AreEqual("-123", parsed.ToString());
        }

    }
}
