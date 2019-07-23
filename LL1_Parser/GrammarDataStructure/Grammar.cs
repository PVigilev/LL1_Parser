using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    class Grammar : IEnumerable<KeyValuePair<NonTerminal, HashSet<Rule>>>
    {
        Dictionary<NonTerminal, HashSet<Rule>> Rules;
        RuleExpressionEvaluator Evaluator;

        public Grammar(Dictionary<NonTerminal, HashSet<Rule>> rules, RuleExpressionEvaluator ev)
        {
            if (rules == null)
                throw new ArgumentNullException($"Set of rules is null");
            if (rules.Count == 0)
                throw new ArgumentException("Set of rules is empty");
            if (ev == null)
                throw new ArgumentNullException("RuleExpressionEvaluator is null");
            Rules = rules;
            Evaluator = ev;
        }

        public IEnumerable<Rule> this[NonTerminal nt]
        {
            get { return Rules[nt]; }
        }
        public IEnumerable<NonTerminal> GetAllNonTerminals()
        {
            return Rules.Keys;
        }

        public IEnumerator<KeyValuePair<NonTerminal, HashSet<Rule>>> GetEnumerator()
        {
            foreach(var kvp in Rules)
            {
                yield return kvp;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public object EvaluateRuleAction(Rule rule, object[] vars)
        {
            return rule.Action.Evaluate(vars, Evaluator);
        }

        
    }
}
