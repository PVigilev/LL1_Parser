using System;
using System.Collections.Generic;

namespace LL1_Parser
{
    /// <summary>
    /// Checks if the grammar has left recursion
    /// </summary>
    class LeftRecursionChecker : GrammarChecker
    {
        public static LeftRecursionChecker Instance = new LeftRecursionChecker();
        private LeftRecursionChecker() { }

        private enum ResultCode { HasTerminal, LeftRecursion, AllNullable };

        /// <summary>
        /// True if the grammar does not contain left recursion
        /// </summary>
        /// <param name="grammar"></param>
        /// <param name="throw_on_false"></param>
        /// <returns>True if the grammar is ok and does not have left recursion;
        /// false - otherwise
        /// </returns>
        public override bool Check(Grammar grammar, bool throw_on_false = false)
        {
            if (grammar == null)
                throw new ArgumentNullException("Grammar instance is null and can not be checked");
            if (grammar.StartSymbol == null)
                throw new GrammarException("Grammar is empty");

            HashSet<NonTerminal> opened = new HashSet<NonTerminal>();
            var result = CheckNonTerminal(grammar, grammar.StartSymbol, opened);
            if (result == ResultCode.LeftRecursion)
            {
                if (throw_on_false)
                    throw new LeftRecursionGrammarException("Grammar has Left Recursion in the definition");
                else
                    return false;
            }
            return true;

        }

        /// <summary>
        /// Check if the rule contains left recursion
        /// </summary>
        /// <param name="grammar"></param>
        /// <param name="rule"></param>
        /// <param name="nt"></param>
        /// <param name="opened"></param>
        /// <returns>
        /// ResultCode.AllNullable iff the rule can accept an empty string
        /// ResultCode.HasTerminal iff the rule always need at least one terminal that is not an empty string
        /// ResultCode.LeftRecursion iff at least one symbol is is in "opened" set and there are no terminals before it
        /// </returns>
        private static ResultCode CheckRule(Grammar grammar, Rule rule, NonTerminal nt, HashSet<NonTerminal> opened)
        {
            // finding left recursion or the first occurence of non-empty string terminal
            foreach (var s in rule)
            {
                if (s == Terminal.EmptyString)
                    continue;
                if (s is Terminal)
                    return ResultCode.HasTerminal;
                else
                {
                    NonTerminal s_as_nt = s as NonTerminal;
                    if (opened.Contains(s_as_nt))
                        return ResultCode.LeftRecursion;
                    var s_result = CheckNonTerminal(grammar, s_as_nt, opened);
                    if (s_result == ResultCode.AllNullable)
                        continue;
                    return s_result;
                }
            }
            return ResultCode.AllNullable;

        }

        /// <summary>
        /// check if the non-terminal has a left recursion in some of it's rules
        /// </summary>
        /// <param name="grm"></param>
        /// <param name="nt"></param>
        /// <param name="opened"></param>
        /// <returns>
        /// ResultCode.LeftRecursion iff there a left recursion occurs in some rule for this grammar
        /// ResultCode.HasTerminal iff each rule generate some terminal that is not an empty-string
        /// ResultCode.AllNullable iff at least one rule generatesan empty string
        /// </returns>
        private static ResultCode CheckNonTerminal(Grammar grm, NonTerminal nt, HashSet<NonTerminal> opened)
        {
            if (opened.Contains(nt))
                return ResultCode.LeftRecursion;
            opened.Add(nt);

            // finding left recursion or check if it is nullable
            foreach (var rule in grm[nt])
            {
                var checkRule_result = CheckRule(grm, rule, nt, opened);
                if (checkRule_result == ResultCode.HasTerminal)
                    continue;
                return checkRule_result;
            }
            opened.Remove(nt);
            return ResultCode.HasTerminal;
        }

    }
}
