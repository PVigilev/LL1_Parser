using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFFParser;


namespace Lambda
{
    public abstract class LExprBasic
    {
    }


    /// <summary>
    /// single variable
    /// </summary>
    public class LVar
    {
        public string Value { get; }
        public LVar(string val)
        {
            Value = val ?? throw new ArgumentNullException();
        }
        public override string ToString()
        {
            return Value;
        }
    }

    /// <summary>
    /// Application of two lambda expressions
    /// </summary>
    public class LApplication
    {
        public LExprBasic What { get; }
        public LExprBasic To { get; }

        public LApplication(LExprBasic w, LExprBasic t)
        {
            What = w ?? throw new ArgumentNullException();
            To = t ?? throw new ArgumentNullException();
        }
        public override string ToString()
        {
            return $"({What.ToString()} {To.ToString()})";
        }
    }

    /// <summary>
    /// Lambda-term
    /// </summary>
    public class LAbstraction
    {
        public LVar Variable { get; }
        public LExprBasic Expression { get; }
        public LAbstraction(LVar var, LExprBasic expr)
        {
            Variable = var ?? throw new ArgumentNullException();
            Expression = expr ?? throw new ArgumentNullException();
        }
        public override string ToString()
        {
            return $"(\\{Variable}. {Expression.ToString()})";
        }
    }
}

namespace BasicTests
{
    using Lambda;
    abstract class AbstractLToken : IToken
    {
        public abstract bool IsCompatible(IToken other);
    }
    class LToken : AbstractLToken
    {
        public enum TokenType { backslash, ob, cb }
        TokenType Type;
        private LToken(TokenType type) { Type = type; }
        public static readonly Dictionary<TokenType, LToken> Instances;
        static LToken()
        {
            Instances = new Dictionary<TokenType, LToken>();
            for (int i = 0; i <= 5; i++)
            {
                Instances.Add((TokenType)i, new LToken((TokenType)i));
            }
        }
        public override bool IsCompatible(IToken other)
        {
            if (other is LToken tkn)
                return Type == tkn.Type;
            return false;
        }
    }

    class TokenLVar : AbstractLToken, ITokenWithValue<string>
    {
        public string Value { get; set; }
        public TokenLVar(string val)
        {
            Value = val ?? throw new ArgumentNullException();
        }
        public override bool IsCompatible(IToken other)
        {
            return other.GetType() == typeof(TokenLVar);
        }
    }


    class LLexer : ILexer<AbstractLToken>
    {
        public Dictionary<string, AbstractLToken> NameTokenTBL = new Dictionary<string, AbstractLToken>
                {
                    {"\\",LToken.Instances[LToken.TokenType.backslash] },
                    {"(",LToken.Instances[LToken.TokenType.ob] },
                    {")",LToken.Instances[LToken.TokenType.cb] },
                    {"var",new TokenLVar("") }
                };
        public IList<AbstractLToken> Tokenize(string str)
        {
            List<AbstractLToken> tokens = new List<AbstractLToken>();
            HashSet<char> separators = new HashSet<char> { '\\', '(', ')' };
            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsWhiteSpace(str[i]))
                    continue;
                else if (str[i] == '\\')
                    tokens.Add(LToken.Instances[LToken.TokenType.backslash]);
                else if (str[i] == '(')
                    tokens.Add(LToken.Instances[LToken.TokenType.ob]);
                else if (str[i] == ')')
                    tokens.Add(LToken.Instances[LToken.TokenType.cb]);
                else
                {
                    int j = i;
                    while (j < str.Length && !char.IsWhiteSpace(str[i])) j++;
                    tokens.Add(new TokenLVar(str.Substring(i, j - i)));
                }
            }
            return tokens;
        }
    }

    [TestClass]
    class LambdaExpressionsTests
    {
        [TestMethod]
        public void ParseLambdaExpr()
        {
            string filename = @"C:\Users\Paul\Desktop\prg\cs\SemesterProject\LL1_Parser\BasicTests\Lamda_calc_grammar.txt";
            string[] lambda_expressions = @"(\x.(xy)) ((\y.y)(\x.(xy))) (x(\x.(λx.x))) (\x.(yz))".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            LLexer lexer = new LLexer();
            string grammar;
            using(System.IO.StreamReader reader = new System.IO.StreamReader(filename))
            {
                grammar = reader.ReadToEnd();
            }
            if (grammar == null || grammar == "")
                Assert.Fail("Cannot read a grammar file");
            var parser = new LL1GrammarParser<LExprBasic, AbstractLToken>(grammar, lexer, lexer.NameTokenTBL, new System.Reflection.Assembly[] { typeof(LExprBasic).Assembly });
            foreach(var str_expr in lambda_expressions)
            {
                var expr = parser.Parse(str_expr);
                Assert.AreEqual(str_expr, expr.ToString());
            }

        }
    }
}
