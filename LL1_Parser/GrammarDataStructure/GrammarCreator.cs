using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
#if DEBUG
    public
#endif
    class GrammarCreator 
    {
        protected struct RulePair
        {
            public Symbol[] Symbols;
            public RuleExpressionFactory[] ExprsFactory;

            public RulePair(Symbol[] symbols, RuleExpressionFactory[] exprsFactory)
            {
                this.Symbols = symbols;
                ExprsFactory = exprsFactory;
            }

            public override bool Equals(object obj)
            {
                return obj is RulePair pair &&
                       EqualityComparer<Symbol[]>.Default.Equals(Symbols, pair.Symbols) &&
                       EqualityComparer<RuleExpressionFactory[]>.Default.Equals(ExprsFactory, pair.ExprsFactory);
            }

            public override int GetHashCode()
            {
                var hashCode = 1926275385;
                hashCode = hashCode * -1521134295 + EqualityComparer<Symbol[]>.Default.GetHashCode(Symbols);
                hashCode = hashCode * -1521134295 + EqualityComparer<RuleExpressionFactory[]>.Default.GetHashCode(ExprsFactory);
                return hashCode;
            }
        }

        /// <summary>
        /// Assemblies that will be passed to each RuleExpressionFactory where the framework finds types and methods
        /// </summary>
        public AssembliesAccessWrapper Assemblies { get; set; }

        /// <summary>
        /// ExpressionEvaluator that will be passed to Grammar instance
        /// </summary>
        public RuleExpressionEvaluator ExpressionEvaluator { get; set; }

        protected NonTerminal _startsymbol = null;
        /// <summary>
        /// NonTerminal that the grammar represents
        /// </summary>
        internal NonTerminal StartSymbol
        {
            get { return _startsymbol; }
            set
            {
                if (!ResultGrammarContentImage.ContainsKey(value))
                    throw new GrammarException($"Starting symbol is undefined in the grammar");
                _startsymbol = value;
            }
        }

        /// <summary>
        /// Tables with name of token (terminal) 
        /// </summary>
        protected Dictionary<string, Terminal> NameTerminalTable = new Dictionary<string, Terminal>();
        protected Dictionary<IToken, Terminal> TokenTerminalTable = new Dictionary<IToken, Terminal>();

        /// <summary>
        /// Table with NonTerminals names and its instances
        /// </summary>
        protected Dictionary<string, NonTerminal> NameNTerminalTable = new Dictionary<string, NonTerminal>();

        /// <summary>
        /// Table with factories of expressions for each rule;
        /// </summary>
        protected Dictionary<NonTerminal, HashSet<RulePair>> ResultGrammarContentImage = new Dictionary<NonTerminal, HashSet<RulePair>>();

        internal GrammarCreator()
        {
            NameTerminalTable.Add("_empty_string_", Terminal.EmptyString);
        }
        internal GrammarCreator(RuleExpressionEvaluator evaluator, AssembliesAccessWrapper assemblies)
            :this()
        {
            ExpressionEvaluator = evaluator ?? throw new ArgumentNullException("Evaluator is null");
            Assemblies = assemblies ?? throw new ArgumentNullException("Assemblies wrapper is null");
        }
        internal GrammarCreator(RuleExpressionEvaluator ev, AssembliesAccessWrapper wrapper, Dictionary<string, Terminal> name_terminal_table, Dictionary<IToken, Terminal> token_terminal_table)
            : this(ev, wrapper)
        {
            if ((name_terminal_table ?? throw new ArgumentNullException($"Table of name/terminal is null")).Count
                != (token_terminal_table ?? throw new ArgumentNullException($"Table token/terminal is null")).Count)
                throw new GrammarException($"Problem with terminal registration");
            NameTerminalTable = name_terminal_table;
            TokenTerminalTable = token_terminal_table;
        }

#if DEBUG
        public
#else
        internal
#endif
        GrammarCreator(RuleExpressionEvaluator ev, AssembliesAccessWrapper wrapper, Dictionary<string, IToken> name_token_table)
            : this(ev, wrapper)
        {
            ExpressionEvaluator = ev ?? throw new ArgumentNullException($"Rule expression evaluator is not able to be null");
            foreach (var kvp in name_token_table)
                RegisterTerminal(kvp.Key, kvp.Value);
        }



        /// <summary>
        /// Register new terminal with the token it is represented
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        internal void RegisterTerminal(string name, IToken token)
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
        /// Register new rule using main information
        /// </summary>
        /// <param name="NonTerminal"></param>
        /// <param name="Symbols"></param>
        /// <param name="action"></param>
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
#if DEBUG
                    nt = new NonTerminal(NonTerminalName);
#else
                    nt = new NonTerminal();
#endif
                    NameNTerminalTable.Add(NonTerminalName, nt);
                }
            }

            


            // resolving symbols in a rule
            Symbol[] symbols = new Symbol[Words.Length];
            for (int i = 0; i < Words.Length; i++)
            {
                NonTerminal nt1;
                Terminal tr;
                if (NameNTerminalTable.TryGetValue(Words[i], out nt1))
                {
                    symbols[i] = nt1;
                }
                else if(NameTerminalTable.TryGetValue(Words[i], out tr))
                {
                    symbols[i] = tr;
                }
                else
                {
#if DEBUG
                    nt1 = new NonTerminal(Words[i]);
#else
                    nt1 = new NonTerminal();
#endif
                    symbols[i] = nt1;
                    NameNTerminalTable.Add(Words[i], nt1);
                }
            }

            if (!ResultGrammarContentImage.ContainsKey(nt))
                ResultGrammarContentImage.Add(nt, new HashSet<RulePair>());
            ResultGrammarContentImage[nt].Add(new RulePair(symbols, actions));

            // setting starting symbol the first one be default
            if (StartSymbol == null)
                StartSymbol = nt;
        }


        public bool IsCorrect(out Exception ex)
        {
            foreach(var nt_ruleimg in ResultGrammarContentImage)
            {
                foreach(var rule in nt_ruleimg.Value)
                {
                    foreach(var symbol in rule.Symbols)
                    {
                        if(symbol is NonTerminal nt)
                        {
                            if (!NameNTerminalTable.ContainsValue(nt))
                            {
                                ex = new GrammarException("There exist NonTerminal on the right-hand side of some rule, such that it is not represented in the table of terminal names");
                                return false;
                            }
                            if (!ResultGrammarContentImage.ContainsKey(nt))
                            {
                                string nt_name ="";
                                foreach(var kvp in NameNTerminalTable)
                                {
                                    if (kvp.Value == nt)
                                    {
                                        nt_name = kvp.Key;
                                        break;
                                    }
                                }
                                ex = new GrammarException($"There exist NonTerminal {nt_name} on the right-hand side of some rule, such that it doesn't have any rule for it");
                                return false;
                            }                               
                            
                        }
                    }
                }
            }
            ex = null;
            return true;
        }

        /// <summary>
        /// Creates a grammar using the current state of the instance
        /// </summary>
        /// <returns> Created instance of Grammar </returns>
        internal Grammar Create()
        {
            Exception exc;
            if (!IsCorrect(out exc))
                throw exc;
            Dictionary<NonTerminal, HashSet<Rule>> Result = new Dictionary<NonTerminal, HashSet<Rule>>();
            foreach (var kvp in ResultGrammarContentImage)
            {
                if (!Result.ContainsKey(kvp.Key))
                    Result.Add(kvp.Key, new HashSet<Rule>());
                foreach (var pair in kvp.Value)
                {
                    RuleExpression[] exprs = new RuleExpression[pair.ExprsFactory.Length];
                    for (int i = 0; i < exprs.Length; i++)
                    {
                        exprs[i] = pair.ExprsFactory[i].Create(Assemblies);
                    }
                    Rule rule = new Rule(pair.Symbols, new RuleAction(exprs));
                    Result[kvp.Key].Add(rule);
                }
            }
            return new Grammar(StartSymbol, Result, ExpressionEvaluator);
        }


        public void SetStartSymbol(string name)
        {
            if (name == null)
                throw new ArgumentNullException("Name of the new start symbol is null");
            NonTerminal nt;
            if (!NameNTerminalTable.TryGetValue(name, out nt))
                throw new GrammarException($"Nonterminal {name} does not exist");
            StartSymbol = nt;
        }


       
    }
}
