using System;
using System.Collections.Generic;

namespace LL1_Parser
{
    /// <summary>
    /// Engine of recursive descent parser
    /// </summary>
    /// <typeparam name="Token">Type that represents token-type for representing terminals and input symbols</typeparam>
    class RecursiveDescentParser<Token> : AbstractParser<Token> where Token : IToken
    {
        /// <summary>
        /// creating parser using defined grammar
        /// </summary>
        /// <param name="grm"></param>
        internal RecursiveDescentParser(Grammar grm) : base(grm)
        {
            LeftRecursionChecker.Instance.Check(grm, true);
        }

        /// <summary>
        /// Parses an input sequence by a grammar passed to constructor
        /// </summary>
        /// <param name="tokens">Sequnce of tokens that will be parsed</param>
        /// <returns>Parsed object created by using grammar-file expressions</returns>
        public override object Parse(IList<Token> tokens)
        {
            int cur = 0;
            object result = ParseNonTerminal(Grammar.StartSymbol, tokens, ref cur);
            if (result == NotParsedObject.Instance)
                throw new FormatException($"Grammar does not generate an input string");
            return result;
        }


        /// <summary>
        /// Trying to parse "tokens" from the position "cur" as a non-termninal "nt" using each rule
        /// </summary>
        /// <param name="nt">Non-terminal symbol (trying it as a root of parsing subtree)</param>
        /// <param name="tokens">Sequence of tokens that have to be parsed</param>
        /// <param name="cur">current position in sequence</param>
        /// <returns>Parsed object</returns>
        private object ParseNonTerminal(NonTerminal nt, IList<Token> tokens, ref int cur)
        {
            object result = NotParsedObject.Instance;
            foreach (var rule in Grammar[nt])
            {
                int backtrack = cur;
                var res = ParseRule(rule, tokens, ref cur);

                if (res != NotParsedObject.Instance)
                {
                    if (result == NotParsedObject.Instance)
                    {
                        result = res;
                        break;
                    }
                    else
                    {
                        throw new AmbiguousGrammarException("There are at least two rules such that they generate an input string");
                    }
                }
                else cur = backtrack;
            }
            return result;
        }

        /// <summary>
        /// Trying to parse a sequence of tokens using "rule"
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="tokens"></param>
        /// <param name="cur"></param>
        /// <returns>parsed object</returns>
        private object ParseRule(Rule rule, IList<Token> tokens, ref int cur)
        {
            if (rule.Length == 1 && rule[0] == Terminal.EmptyString)
                return Grammar.EvaluateRuleAction(rule, new object[0]);

            object[] result = new object[rule.Length];
            for (int i = 0; i < rule.Length; i++)
            {
                if (rule[i] is Terminal terminal)
                {
                    if (cur >= tokens.Count || !terminal.IsCompatiable(tokens[cur]))
                    {
                        return NotParsedObject.Instance;
                    }
                    else
                    {
                        result[i] = tokens[cur] is ITokenWithValue<object> token ? token.Value : null;
                        cur++;
                    }
                }
                else //if (rule[i] is NonTerminal)
                {
                    NonTerminal nonterm = (NonTerminal)rule[i];
                    result[i] = ParseNonTerminal(nonterm, tokens, ref cur);
                    if (result[i] == NotParsedObject.Instance)
                    {
                        return NotParsedObject.Instance;
                    }
                }
            }
            return Grammar.EvaluateRuleAction(rule, result);
        }

    }
}
