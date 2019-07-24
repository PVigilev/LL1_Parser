using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{

    class GrammarCreator
    {
        RuleExpressionEvaluator ExpressionEvaluator;
        public NonTerminal StartSymbol { get { return StartSymbol; }
            set
            {
                if (!Result.ContainsKey(value))
                    throw new GrammarException($"Starting symbol is undefined in the grammar");
                StartSymbol = value;
            }
        }
        Dictionary<NonTerminal, HashSet<Rule>> Result = new Dictionary<NonTerminal, HashSet<Rule>>();
        public GrammarCreator(RuleExpressionEvaluator ev)=>
            ExpressionEvaluator = ev ?? throw new ArgumentNullException($"Rule expression evaluator is not able to be null");

        /// <summary>
        /// Table with name of token (terminal) and its creators
        /// </summary>
        Dictionary<string, Terminal> NameTerminalTable = new Dictionary<string, Terminal>();
        Dictionary<IToken, Terminal> TokenTerminalTable = new Dictionary<IToken, Terminal>();
        /// <summary>
        /// Table with NonTerminals name and its instance
        /// </summary>
        Dictionary<string, NonTerminal> NameNTerminalTable = new Dictionary<string, NonTerminal>();
        

        // register new terminal
        public void RegisterTerminal(string name, IToken token)
        {
            if (NameNTerminalTable.ContainsKey(name))
                throw new ParserErrorException($"There already exists the non-terminal with name {name}");
            if (NameTerminalTable.ContainsKey(name))
                throw new FormatException($"Grammar-file already contains definition of terminal {name}");
            if (TokenTerminalTable.ContainsKey(token))
                throw new ParserErrorException($"The token that represents {name}-terminal already represents other terminal");

            NameTerminalTable.Add(name, new Terminal(token));
        }

        /// <summary>
        /// Creates and register new rule of grammar
        /// </summary>
        /// <param name="NonTerminalName"></param>
        /// <param name="Words"></param>
        /// <param name="actions"></param>
        public void RegisterRule(string NonTerminalName, string[] Words, RuleExpressionFactory[] actions)
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
            NonTerminal nt;
            if (!NameNTerminalTable.TryGetValue(NonTerminalName, out nt))
                throw new ParserErrorException($"Undefined non-terminal {NonTerminalName}");

            // resolving symbols int a rule
            Symbol[] symbols = new Symbol[Words.Length];
            for(int i = 0; i < Words.Length; i++)
            {
                Symbol cur;
                NonTerminal ntCur;
                if (!NameNTerminalTable.TryGetValue(Words[i], out ntCur))
                {
                    Terminal tCur;
                    if (!NameTerminalTable.TryGetValue(Words[i], out tCur))
                        throw new Exception($"Undefined symbol with name {Words[i]}");
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

            // creatinf=g and adding new rule
            if (!Result.ContainsKey(nt))
                Result.Add(nt, new HashSet<Rule>());
            Result[nt].Add(new Rule(symbols, new RuleAction(exprs, ExpressionEvaluator)));
        }

        /// <summary>
        /// Creates a grammar using the current state of the instance
        /// </summary>
        /// <returns> Created instance of Grammar </returns>
        public Grammar Create()
        {
            if (Result.Count == 0)
                throw new GrammarException($"Grammar must be non-empty");
            if (StartSymbol == null)
                throw new GrammarException($"Grammars starting symbol is null");
            return new Grammar(StartSymbol, Result, ExpressionEvaluator);
        }
        
    }
}
