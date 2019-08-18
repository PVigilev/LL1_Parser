﻿using LL1_Parser.Initialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{

    public sealed class Grammar : IEnumerable<KeyValuePair<NonTerminal, HashSet<Rule>>>
    {
        public readonly NonTerminal StartSymbol;
        Dictionary<NonTerminal, HashSet<Rule>> Rules;
        RuleExpressionEvaluator Evaluator;

        public Grammar(NonTerminal start, Dictionary<NonTerminal, HashSet<Rule>> rules, RuleExpressionEvaluator ev)
        {
            if (rules == null)
                throw new ArgumentNullException($"Set of rules is null");
            if (rules.Count == 0)
                throw new ArgumentException("Set of rules is empty");
            Rules = rules ?? throw new ArgumentNullException("Set of rules is null");
            Evaluator = ev ?? throw new ArgumentNullException("RuleExpressionEvaluator is null");
            if (!Rules.ContainsKey(start))
                throw new GrammarException($"Starting non-terminal symbol is not defined in the grammar");
            StartSymbol = start ?? throw new ArgumentNullException("Start symbol is null");
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
