﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{

    public class GrammarCreator
    {
        RuleExpressionEvaluator ExpressionEvaluator;
        private NonTerminal _startsymbol = null;
        internal NonTerminal StartSymbol
        {
            get { return _startsymbol; }
            set
            {
                if (!Result.ContainsKey(value))
                    throw new GrammarException($"Starting symbol is undefined in the grammar");
                _startsymbol = value;
            }
        }
        public void SetStartSymbol (string name)
        {
            if (!NameNTerminalTable.ContainsKey(name))
                throw new GrammarException($"Unknown nonterminal {name}");
            StartSymbol = NameNTerminalTable[name];
        }
        Dictionary<NonTerminal, HashSet<Rule>> Result = new Dictionary<NonTerminal, HashSet<Rule>>();

#if DEBUG
        public
#endif
        /// <summary>
        /// Table with name of token (terminal) and its creators
        /// </summary>
        Dictionary<string, Terminal> NameTerminalTable = new Dictionary<string, Terminal>();
        Dictionary<IToken, Terminal> TokenTerminalTable = new Dictionary<IToken, Terminal>();

#if DEBUG
        public
#endif
        /// <summary>
        /// Table with NonTerminals name and its instance
        /// </summary>
        Dictionary<string, NonTerminal> NameNTerminalTable = new Dictionary<string, NonTerminal>();

        internal GrammarCreator(RuleExpressionEvaluator ev)
        {
            ExpressionEvaluator = ev ?? throw new ArgumentNullException($"Rule expression evaluator is not able to be null");
            NameTerminalTable.Add("_empty_string_", Terminal.EmptyString);
        }
        internal GrammarCreator(RuleExpressionEvaluator ev, Dictionary<string, Terminal> name_terminal_table, Dictionary<IToken, Terminal> token_terminal_table)
        {
            ExpressionEvaluator = ev ?? throw new ArgumentNullException($"Rule expression evaluator is not able to be null");
            if ((name_terminal_table ?? throw new ArgumentNullException($"Table of name/terminal is null")).Count
                != (token_terminal_table ?? throw new ArgumentNullException($"Table token/terminal is null")).Count)
                throw new GrammarException($"Problem with terminal registration");
            NameTerminalTable = name_terminal_table;
            TokenTerminalTable = token_terminal_table;
        }
        internal GrammarCreator(RuleExpressionEvaluator ev, Dictionary<string, IToken> name_token_table)
        {
            ExpressionEvaluator = ev ?? throw new ArgumentNullException($"Rule expression evaluator is not able to be null");
            foreach (var kvp in name_token_table)
                RegisterTerminal(kvp.Key, kvp.Value);
        }

        public GrammarCreator(Dictionary<string, IToken> name_token_table) : this(new REEvaluator(), name_token_table)  { }
        public GrammarCreator(Dictionary<string, Terminal> name_terminal_table, Dictionary<IToken, Terminal> token_terminal_table) : this(new REEvaluator(), name_terminal_table, token_terminal_table) { }
        // register new terminal
        public void RegisterTerminal(string name, IToken token)
        {
            if (NameNTerminalTable.ContainsKey(name))
                throw new ParserErrorException($"There already exists the non-terminal with name {name}");
            if (NameTerminalTable.ContainsKey(name))
                throw new FormatException($"Grammar-file already contains definition of terminal {name}");
            if (TokenTerminalTable.ContainsKey(token))
                throw new ParserErrorException($"The token that represents {name}-terminal already represents other terminal");

            Terminal t = new Terminal(token);
            NameTerminalTable.Add(name, t);
            TokenTerminalTable.Add(token, t);
        }

        /// <summary>
        /// Creates and register new rule of grammar
        /// </summary>
        /// <param name="NonTerminalName"></param>
        /// <param name="Words"></param>
        /// <param name="actions"></param>
        internal void RegisterRule(string NonTerminalName, string[] Words, RuleExpressionFactory[] actions)
        {
            if (NonTerminalName == null)
                throw new ArgumentNullException($"Name of non-terminal is null");
            if (NonTerminalName.Length == 0)
                throw new ArgumentException("Non-terminal name must not be empty");
            if (Words == null)
                throw new ArgumentNullException($"Sequence of symbols is null");
            if (Words.Length == 0)
                throw new ArgumentException($"The sequence of symbols in a rule must not be empty");
            if (actions.Length == 0)
                throw new ArgumentException($"The sequence of expressions in a rule must not be empty");

            // if there is no symbol with NonTerminalName we define it
            // otherwise - throwing an exception
            NonTerminal nt;
            if (!NameNTerminalTable.TryGetValue(NonTerminalName, out nt))
            {
                if (NameTerminalTable.ContainsKey(NonTerminalName))
                    throw new GrammarException($"Symbol {NonTerminalName} is already defined as a Terminal");
                else
                {
                    nt = new NonTerminal();
                    NameNTerminalTable.Add(NonTerminalName, nt);
                }
            }
            


            // resolving symbols in a rule
            Symbol[] symbols = new Symbol[Words.Length];
            for(int i = 0; i < Words.Length; i++)
            {
                Symbol cur;
                NonTerminal ntCur;
                if (!NameNTerminalTable.TryGetValue(Words[i], out ntCur))
                {
                    Terminal tCur;
                    if (!NameTerminalTable.TryGetValue(Words[i], out tCur))
                    {
                        cur = new NonTerminal();
                        NameNTerminalTable.Add(Words[i], (NonTerminal) cur);
                    }
                    else cur = tCur;
                }
                else cur = ntCur;
                symbols[i] = cur;
            }

            // creating expressions
            RuleExpression[] exprs = new RuleExpression[actions.Length];
            for(int i = 0; i < exprs.Length; i++)
            {
                exprs[i] = actions[i].Create();
            }

            // creating and adding new rule
            if (!Result.ContainsKey(nt))
                Result.Add(nt, new HashSet<Rule>());
            Result[nt].Add(new Rule(symbols, new RuleAction(exprs, ExpressionEvaluator)));
        }


        /// <summary>
        /// Creates a grammar using the current state of the instance
        /// </summary>
        /// <returns> Created instance of Grammar </returns>
        internal Grammar Create()
        {
            if (Result.Count == 0)
                throw new GrammarException($"Grammar must be non-empty");
            if (StartSymbol == null)
                throw new GrammarException($"Grammars starting symbol is null");
            return new Grammar(StartSymbol, Result, ExpressionEvaluator);
        }
        
    }
}
