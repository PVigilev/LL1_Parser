using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{

    public class GrammarCreator : AbstractGrammarCreator
    {
        internal override NonTerminal StartSymbol
        {
            get { return _startsymbol; }
            set
            {
                if (!Result.ContainsKey(value))
                    throw new GrammarException($"Starting symbol is undefined in the grammar");
                _startsymbol = value;
            }
        }
        internal GrammarCreator(RuleExpressionEvaluator ev, AssembliesAccessWrapper wrapper) : base(ev, wrapper) { }
        internal GrammarCreator(RuleExpressionEvaluator ev, AssembliesAccessWrapper wrapper, Dictionary<string, Terminal> name_terminal_table, Dictionary<IToken, Terminal> token_terminal_table)
            : base(ev, wrapper)
        {
            if ((name_terminal_table ?? throw new ArgumentNullException($"Table of name/terminal is null")).Count
                != (token_terminal_table ?? throw new ArgumentNullException($"Table token/terminal is null")).Count)
                throw new GrammarException($"Problem with terminal registration");
            NameTerminalTable = name_terminal_table;
            TokenTerminalTable = token_terminal_table;
        }
        internal GrammarCreator(RuleExpressionEvaluator ev, AssembliesAccessWrapper wrapper, Dictionary<string, IToken> name_token_table)
            : base(ev, wrapper)
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
        internal override void RegisterTerminal(string name, IToken token)
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
        /// <param name="NonTerminalName"></param>
        /// <param name="Words"></param>
        /// <param name="actions"></param>
        internal override void RegisterRule(string NonTerminalName, string[] Words, RuleExpressionFactory[] actions)
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

        public override void SetStartSymbol(string name)
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
