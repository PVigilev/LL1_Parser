using System;
using System.Collections.Generic;
using LL1_Parser;
using Lambda;

namespace LamdaExpressionComparement
{
    abstract class AbstractLToken : IToken
    {
        public abstract bool IsCompatible(IToken other);
    }
    class LToken : AbstractLToken
    {
        public enum TokenType { backslash, ob, cb, dot }
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
                    {".", LToken.Instances[LToken.TokenType.dot] },
                    {"var",new TokenLVar("") }
                };
        public IList<AbstractLToken> Tokenize(string str)
        {
            List<AbstractLToken> tokens = new List<AbstractLToken>();
            HashSet<char> separators = new HashSet<char> { '\\', '(', ')', '.', ' ', '\n', '\t', '\n', '\r' };
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
                else if (str[i] == '.')
                    tokens.Add(LToken.Instances[LToken.TokenType.dot]);
                else
                {
                    int j = i;
                    while (j < str.Length && !separators.Contains(str[j])) j++;
                    tokens.Add(new TokenLVar(str.Substring(i, j - i)));
                    i = j - 1;
                }
            }
            return tokens;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            string filename = @"C:\Users\Paul\Desktop\prg\cs\SemesterProject\LL1_Parser\LamdaExpressionComparement\Lamda_calc_grammar.txt";
            string l_expr = @"(\x.(\y.(x(y((\z.t)z)))))";
            LLexer lexer = new LLexer();
            string grammar;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(filename))
            {
                grammar = reader.ReadToEnd();
            }
            var parser = new LL1GrammarParser<LExprBasic, AbstractLToken>(grammar, lexer, lexer.NameTokenTBL, new System.Reflection.Assembly[] { typeof(LExprBasic).Assembly });


            Console.WriteLine("Expected\t\tActual");
            var expr = parser.Parse(l_expr);
            Console.WriteLine($"{l_expr}\t\t{expr.ToString()}");
            
        }
    }
}
