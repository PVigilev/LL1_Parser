using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
#if DEBUG
    public
#endif
    class RDParser<Token> : AbstractParser<Token> where Token : IToken
    {
        internal RDParser(Grammar grm) : base(grm) { }
        public override object Parse(IList<Token> tokens)
        {
            int cur = 0;
            object result = ParseNonTerminal(Grammar.StartSymbol, tokens, ref cur);
            if (result == NotParsedObject.Instance)
                throw new FormatException($"Grammar does not generate an input sequence");
            return result;
        }
        
        private object ParseNonTerminal(NonTerminal nt, IList<Token> tokens, ref int cur)
        {
            object result = NotParsedObject.Instance;
            foreach(var rule in Grammar[nt])
            {
                object[] ParsingResult = ParseRule(rule, tokens, ref cur);
                if(ParsingResult != null)
                {
                    if (result != NotParsedObject.Instance)
                        throw new AmbiguousGrammarException($"There are at least two ways to generate input");
                    result = Grammar.EvaluateRuleAction(rule, ParsingResult);
                }
            }
            return result;
        }

        private object[] ParseRule(Rule rule, IList<Token> tokens, ref int cur)
        {
            int backtrack = cur;
            object[] result = new object[rule.Length];
            for(int i = 0; i < rule.Length; i++)
            {
                if(rule[i] is Terminal term)
                {
                    if (term == Terminal.EmptyString)
                        result[i] = null;
                    else if (term.IsCompatiable(tokens[cur]))
                    {
                        if (tokens[cur] is ITokenWithValue<object> tkn)
                            result[i] = tkn.Value;
                        else
                            result[i] = null;
                        cur++;
                        continue;
                    }
                    else
                    {
                        cur = backtrack;
                        return null;
                    }
                }

                else
                {
                    NonTerminal nt = (NonTerminal)rule[i];
                    object parsed = ParseNonTerminal(nt, tokens, ref cur);
                    if (parsed == NotParsedObject.Instance)
                    {
                        cur = backtrack;
                        return null;
                    }
                    else
                        result[i] = parsed;
                }
            }
            return result;
        }
    }
}
