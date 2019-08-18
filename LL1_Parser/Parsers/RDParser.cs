using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{

    public class RecursiveDescentParser<Token> : AbstractParser<Token> where Token : IToken
    {
        internal RecursiveDescentParser(Grammar grm) : base(grm)
        {
            LeftRecursionChecker.Instance.Check(grm, true);
        }
        public override object Parse(IList<Token> tokens)
        {
            int cur = 0;
            object result = ParseNonTerminal(Grammar.StartSymbol, tokens, ref cur);
            if (result == NotParsedObject.Instance)
                throw new FormatException($"Grammar does not generate an input string");
            return result;
        }
        

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


        private object ParseRule(Rule rule, IList<Token> tokens, ref int cur)
        {
            if (rule.Length == 1 && rule[0] == Terminal.EmptyString)
                return Grammar.EvaluateRuleAction(rule, new object[0]);

            object[] result = new object[rule.Length];
            for(int i = 0; i < rule.Length; i++)
            {
                if(rule[i] is Terminal terminal)
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
                    if(result[i] == NotParsedObject.Instance)
                    {
                        return NotParsedObject.Instance;
                    }
                }
            }
            return Grammar.EvaluateRuleAction(rule, result);
        }

    }
}
