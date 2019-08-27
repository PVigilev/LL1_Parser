using System;
using System.Collections.Generic;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BasicTests")]
namespace LL1_Parser.Initialization
{
    /// <summary>
    /// Parse string with the grammar 
    /// </summary>
    static class GrammarDefinitionParser
    {
        /// <summary>
        /// Base procedure to parsing whole grammar
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="result"></param>
        public static void ParseGrammarBase(IList<AbstractToken> tokens, ref GrammarCreator result)
        {
            if (result == null)
                result = new GrammarCreator();
            int cur = 0;
            List<ValueTuple<string, string[], RuleExpressionFactory[]>> grammarContent;
            try
            {
                grammarContent = ParseAllGrammar(tokens, ref cur);
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new ParserErrorException("Unexpected end of file", ex);
            }

            foreach (var rule_content in grammarContent)
                result.RegisterRule(rule_content.Item1, rule_content.Item2, rule_content.Item3);
        }

        /// <summary>
        /// parses part of the grammar
        /// Grammar : Rule Grammar1
        /// Grammar1: Rule Grammar1 | emptystring
        /// </summary>
        /// <returns></returns>
        public static List<ValueTuple<string, string[], RuleExpressionFactory[]>> ParseAllGrammar(IList<AbstractToken> tokens, ref int cur)
        {
            List<ValueTuple<string, string[], RuleExpressionFactory[]>> result = new List<(string, string[], RuleExpressionFactory[])>(1);
            do
            {
                result.Add(ParseRule(tokens, ref cur));
            } while (cur < tokens.Count);
            return result;
        }

        /// <summary>
        /// Parses part of the grammar:
        /// Rule : symbol ':' Symbols '{' Exprs '}'
        /// 
        /// ends on the after '}'
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="cur"></param>
        /// <returns></returns>
        public static ValueTuple<string, string[], RuleExpressionFactory[]> ParseRule(IList<AbstractToken> tokens, ref int cur)
        {
            ValueTuple<string, string[], RuleExpressionFactory[]> result = new ValueTuple<string, string[], RuleExpressionFactory[]>();

            // parse name of non-terminal
            if (!(tokens[cur] is TokenSymbol tknName))
                throw new ParserErrorException($"Syntax error. Symbol-name expected");
            result.Item1 = tknName.Value;
            cur++;
            if (tokens[cur] != SimpleToken.Instances[SimpleToken.TokenType.column])
                throw new ParserErrorException($"Syntax error. Symbol ':' expected");
            cur++;
            List<string> symbols = new List<string>(1);

            // parse symbol-sequence
            do
            {
                symbols.Add(tokens[cur++] is TokenSymbol symbol ? symbol.Value : throw new ParserErrorException("SyntaxError. symbol or '{' are expected"));
            } while (tokens[cur] != SimpleToken.Instances[SimpleToken.TokenType.OCB]);
            result.Item2 = symbols.ToArray();
            cur++;

            // expression parsing
            RuleExpressionFactory[] Exprs = ParseExprs(tokens, ref cur).ToArray();
            if (tokens[cur] != SimpleToken.Instances[SimpleToken.TokenType.CCB])
                throw new ParserErrorException("Syntax error. The '}' expected");
            cur++;
            result.Item3 = Exprs;
            return result;
        }

        /// <summary>
        /// Parses parts of the grammar:
        /// Exprs : Expr ';' Exprs1
        /// Exprs1: Expr ';' Exprs1 | emptystring
        /// 
        /// cur is '}'
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="cur"></param>
        /// <returns></returns>
        public static List<RuleExpressionFactory> ParseExprs(IList<AbstractToken> tokens, ref int cur)
        {
            List<RuleExpressionFactory> result = new List<RuleExpressionFactory>(1);
            do
            {
                RuleExpressionFactory expr = ParseExpr(tokens, ref cur);
                if (tokens[cur++] != SimpleToken.Instances[SimpleToken.TokenType.semicolumn])
                    throw new ParserErrorException("Syntax error. ';' is expected after the end of any expression");
                result.Add(expr);
            } while (tokens[cur] != SimpleToken.Instances[SimpleToken.TokenType.CCB]);
            return result;
        }

        /// <summary>
        /// Parses part of the grammar:
        /// Expr : new Name '(' Args ')'
        /// Expr : static Name '(' Args ')'
        /// Expr : Name '(' Expr Args1 ')'
        /// Expr : Literal
        /// 
        /// next after the last of the expressions token
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="cur"></param>
        /// <returns></returns>
        public static RuleExpressionFactory ParseExpr(IList<AbstractToken> tokens, ref int cur)
        {
            RuleExpressionFactory result = null;
            if (tokens[cur] == TokenKeyWord.Instances[TokenKeyWord.KeyWordType.newKW])
            {
                cur++;
                string name = ParseName(tokens, ref cur);
                if (tokens[cur] != SimpleToken.Instances[SimpleToken.TokenType.OB])
                    throw new ParserErrorException("Syntax error. '(' expected");
                cur++;
                var args = ParseArgs(tokens, ref cur);
                result = new ConstructorCallingCreator(name, args.ToArray());
                if (tokens[cur] != SimpleToken.Instances[SimpleToken.TokenType.CB])
                    throw new ParserErrorException("Syntax error. ')' expected");
            }
            else if (tokens[cur] == TokenKeyWord.Instances[TokenKeyWord.KeyWordType.staticKW])
            {
                cur++;
                string name = ParseName(tokens, ref cur);
                if (tokens[cur] != SimpleToken.Instances[SimpleToken.TokenType.OB])
                    throw new ParserErrorException("Syntax error. '(' expected");
                cur++;
                var args = ParseArgs(tokens, ref cur);
                result = new StaticMethodCallingCreator(name, args.ToArray());
                if (tokens[cur] != SimpleToken.Instances[SimpleToken.TokenType.CB])
                    throw new ParserErrorException("Syntax error. ')' expected");
            }
            else if (tokens[cur] is TokenSymbol)
            {
                string name = ParseName(tokens, ref cur);
                if (tokens[cur] != SimpleToken.Instances[SimpleToken.TokenType.OB])
                    throw new ParserErrorException("Syntax error. '(' expected");
                cur++;
                var args = ParseArgs(tokens, ref cur);
                var context = args[0];
                args = args.GetRange(1, args.Count - 1);
                result = new InstanceMethodCallingCreator(name, args.ToArray(), context);
                if (tokens[cur] != SimpleToken.Instances[SimpleToken.TokenType.CB])
                    throw new ParserErrorException("Syntax error. ')' expected");
            }
            else
            {
                result = ParseLiteral(tokens, ref cur);
            }
            cur++;
            return result;
        }

        /// <summary>
        /// Parses part of the grammar:
        /// Args : Expr Args1
        /// Args1: ',' Expr Args1 | emptystring
        /// 
        /// 
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="cur"></param>
        /// <returns></returns>
        public static List<RuleExpressionFactory> ParseArgs(IList<AbstractToken> tokens, ref int cur, bool AllowEmpty = true)
        {
            var result = new List<RuleExpressionFactory>();
            if (AllowEmpty && tokens[cur] == SimpleToken.Instances[SimpleToken.TokenType.CB])
                return result;
            do
            {
                var expr = ParseExpr(tokens, ref cur);
                result.Add(expr);
            } while (tokens[cur] == SimpleToken.Instances[SimpleToken.TokenType.comma] && cur++ < tokens.Count);
            return result;
        }

        /// <summary>
        /// Parses literal
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="cur"></param>
        /// <returns></returns>
        public static RuleExpressionFactory ParseLiteral(IList<AbstractToken> tokens, ref int cur)
        {
            RuleExpressionFactory result = null;
            if (tokens[cur] is TokenInt tknInt)
                result = new ConstantCreator<int>((int)tknInt.Value);
            else if (tokens[cur] is TokenDouble tknDouble)
                result = new ConstantCreator<double>((double)tknDouble.Value);
            else if (tokens[cur] is TokenString tknString)
                result = new ConstantCreator<string>((string)tknString.Value);
            else if (tokens[cur] is TokenVarId tknVarId)
                result = new VariableCreator((uint)tknVarId.Value);
            else throw new ParserUnknownTokenException("Syntex error. Unknown token");
            return result;
        }

        /// <summary>
        /// Parses name of a type or method
        /// Name : symbol Symbols
        /// Symbols : '.' symbol Symbols | emptystring
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="cur"></param>
        /// <returns></returns>
        public static string ParseName(IList<AbstractToken> tokens, ref int cur)
        {
            StringBuilder result = new StringBuilder();
            do
            {
                if (tokens[cur] is TokenSymbol symbol)
                {
                    result.Append(symbol.Value);
                    cur++;
                    if (tokens[cur] == SimpleToken.Instances[SimpleToken.TokenType.dot])
                        result.Append('.');
                }
                else throw new ParserErrorException("Syntax error. Wrong name-format");
            } while (tokens[cur] == SimpleToken.Instances[SimpleToken.TokenType.dot] && cur++ < tokens.Count);
            return result.ToString();
        }
    }
}
