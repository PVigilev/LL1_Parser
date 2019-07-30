using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{

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
        public AssembliesAccessWrapper Assemblies
        {
            get
            {
                return Assemblies;
            }
            set
            {
                if (Assemblies == null)
                    Assemblies = value ?? throw new ArgumentNullException($"Assemblies is null");
            }
        }

        /// <summary>
        /// ExpressionEvaluator that will be passed to Grammar instance
        /// </summary>
        public RuleExpressionEvaluator ExpressionEvaluator
        {
            get
            {
                return ExpressionEvaluator;
            }
            set
            {
                if (ExpressionEvaluator == null)
                    ExpressionEvaluator = value ?? throw new ArgumentNullException($"Assemblies is null");
            }
        }

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

        internal GrammarCreator() { }
        internal GrammarCreator(RuleExpressionEvaluator evaluator, AssembliesAccessWrapper assemblies)
        {
            ExpressionEvaluator = evaluator ?? throw new ArgumentNullException("Evaluator is null");
            Assemblies = assemblies ?? throw new ArgumentNullException("Assemblies wrapper is null");
            NameTerminalTable.Add("_empty_string_", Terminal.EmptyString);
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
        internal GrammarCreator(RuleExpressionEvaluator ev, AssembliesAccessWrapper wrapper, Dictionary<string, IToken> name_token_table)
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
                    nt = new NonTerminal();
                    NameNTerminalTable.Add(NonTerminalName, nt);
                }
            }



            // resolving symbols in a rule
            Symbol[] symbols = new Symbol[Words.Length];
            for (int i = 0; i < Words.Length; i++)
            {
                Symbol cur;
                NonTerminal ntCur;
                if (!NameNTerminalTable.TryGetValue(Words[i], out ntCur))
                {
                    Terminal tCur;
                    if (!NameTerminalTable.TryGetValue(Words[i], out tCur))
                    {
                        cur = new NonTerminal();
                        NameNTerminalTable.Add(Words[i], (NonTerminal)cur);
                    }
                    else cur = tCur;
                }
                else cur = ntCur;
                symbols[i] = cur;
            }

            if (!ResultGrammarContentImage.ContainsKey(nt))
                ResultGrammarContentImage.Add(nt, new HashSet<RulePair>());
            ResultGrammarContentImage[nt].Add(new RulePair(symbols, actions));
        }

        /// <summary>
        /// Creates a grammar using the current state of the instance
        /// </summary>
        /// <returns> Created instance of Grammar </returns>
        internal Grammar Create()
        {
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
