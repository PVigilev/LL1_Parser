using System;
using System.Collections.Generic;
using System.Text;
using LL1_Parser.Bootstrap;


namespace LL1_Parser
{
    public abstract class AbstractGrammarCreator
    {
        public Grammar GrammarOfGrammars { get; private set; }
        /// <summary>
        /// Assemblies that will be passed to each RuleExpressionFactory where the framework finds types and methods
        /// </summary>
        protected AssembliesAccessWrapper Assemblies;
        /// <summary>
        /// Result content of the grammar
        /// </summary>
        protected Dictionary<NonTerminal, HashSet<Rule>> Result = new Dictionary<NonTerminal, HashSet<Rule>>();
        /// <summary>
        /// ExpressionEvaluator that will be passed to Grammar instance
        /// </summary>
        protected RuleExpressionEvaluator ExpressionEvaluator;
        protected NonTerminal _startsymbol = null;
        /// <summary>
        /// NonTerminal that the grammar represents
        /// </summary>
        internal abstract NonTerminal StartSymbol { get; set; }

        /// <summary>
        /// Tables with name of token (terminal) 
        /// </summary>
        protected Dictionary<string, Terminal> NameTerminalTable = new Dictionary<string, Terminal>();
        protected Dictionary<IToken, Terminal> TokenTerminalTable = new Dictionary<IToken, Terminal>();

        /// <summary>
        /// Table with NonTerminals names and its instances
        /// </summary>
        protected Dictionary<string, NonTerminal> NameNTerminalTable = new Dictionary<string, NonTerminal>();

        public AbstractGrammarCreator(RuleExpressionEvaluator evaluator, AssembliesAccessWrapper assemblies)
        {
            ExpressionEvaluator = evaluator ?? throw new ArgumentNullException("Evaluator is null");
            Assemblies = assemblies ?? throw new ArgumentNullException("Assemblies wrapper is null");
            NameTerminalTable.Add("_empty_string_", Terminal.EmptyString);
            GrammarOfGrammars = Initializer.CreateDefaultGrammarCreator(assemblies, evaluator).Create();
        }

        /// <summary>
        /// Register new terminal with the token it is represented
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        internal abstract void RegisterTerminal(string name, IToken token);

        /// <summary>
        /// Register new rule using main information
        /// </summary>
        /// <param name="NonTerminal"></param>
        /// <param name="Symbols"></param>
        /// <param name="action"></param>
        internal abstract void RegisterRule(string NonTerminal, string[] Symbols, RuleExpressionFactory[] action);


#if DEBUG
        public
#else
        internal
#endif
        /// <summary>
        /// Creates a grammar using the current state of the instance
        /// </summary>
        /// <returns> Created instance of Grammar </returns>
        virtual Grammar Create()
        {
            if (Result.Count == 0)
                throw new GrammarException($"Grammar must be non-empty");
            if (StartSymbol == null)
                throw new GrammarException($"Grammars starting symbol is null");
            if (!IsCorrect())
                throw new GrammarException($"Grammar is not correct. Check that every symbol is defined and each non-terminal has at least one rule.");
            return new Grammar(StartSymbol, Result, ExpressionEvaluator);
        }
        public bool IsCorrect()
        {
            foreach(var NTrulePair in Result)
            {
                foreach(var rule in NTrulePair.Value)
                {
                    foreach(Symbol s in rule)
                    {
                        if(s is NonTerminal nt)
                        {
                            if (!Result.ContainsKey(nt))
                                return false;
                        }
                        else
                        {
                            if (!TokenTerminalTable.ContainsKey((s as Terminal).CompatibleToken))
                                return false;
                        }
                    }
                }
                    
            }
            return true;
        }
        public abstract void SetStartSymbol(string name);

#if DEBUG
        public
#else
        internal 
#endif
        static class Initializer
        {
            public class RuleTriple
            {
                public string NonTerm;
                public string[] Symbols;
                public RuleExpressionFactory[] Expressions;
            }
            public static GrammarCreator CreateDefaultGrammarCreator(AssembliesAccessWrapper wrapper, RuleExpressionEvaluator ExpressionEvaluator)
            {
                GrammarCreator grm = new GrammarCreator(new REEvaluator(), wrapper);
                foreach (var pair in SimpleToken.Instances)
                {
                    grm.RegisterTerminal(pair.Key.ToString(), pair.Value);
                }
                grm.RegisterTerminal("string", new TokenString(""));
                grm.RegisterTerminal("int", new TokenInt(0));
                grm.RegisterTerminal("double", new TokenDouble(0));
                grm.RegisterTerminal("symbol", new TokenName(""));
                grm.RegisterTerminal("id", new TokenVarId(0));
                grm.RegisterTerminal("new", TokenKeyWord.Instances[TokenKeyWord.KeyWordType.newKW]);
                grm.RegisterTerminal("static", TokenKeyWord.Instances[TokenKeyWord.KeyWordType.staticKW]);



                // registering rules
                grm.RegisterRule("Grammar", "Rule Grammar1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(RegisterRuleInGrammar), new DelegateRuleExpressionCreator(MakeSymbolStart) });
                grm.RegisterRule("Grammar1", "Rule Grammar1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(RegisterRuleInGrammar) });
                grm.RegisterRule("Grammar1", "_empty_string_".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((objs) => ReturnGrammarCreator(objs, wrapper, ExpressionEvaluator)) });
                grm.RegisterRule("Rule", "symbol column Symbols OCB Exprs CCB".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(CreateRuleTriple) });
                grm.RegisterRule("Symbols", "symbol Symbols1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 1)) });
                grm.RegisterRule("Symbols1", "symbol Symbols1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 1)) });
                grm.RegisterRule("Symbols", "_empty_string_".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnNewListString) });
                grm.RegisterRule("Exprs", "Expr semicolumn Exprs1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 2)) });
                grm.RegisterRule("Exprs1", "Expr semicolumn Exprs1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 2)) });
                grm.RegisterRule("Exprs", "_empty_string_".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnNewListExpr) });
                grm.RegisterRule("Expr", "newKeyWord Name OB Args CB".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((objs) => CreateConstructorCalling(objs, wrapper)) });
                grm.RegisterRule("Expr", "staticKeyWord Name OB Args CB".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((objs) => CreateStaticMethodCalling(objs, wrapper)) });
                grm.RegisterRule("Expr", "Name OB Expr Args1 CB".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((objs) => CreateInstanceMethodCalling(objs, wrapper)) });
                grm.RegisterRule("Expr", "Literal".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnLiteral) });
                grm.RegisterRule("Name", "symbol dot Name1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 2)) });
                grm.RegisterRule("Name1", "symbol dot Name1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 2)) });
                grm.RegisterRule("Name1", "_empty_string_".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnNewListString) });
                grm.RegisterRule("Args", "Expr Args1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 1)) });
                grm.RegisterRule("Args", "_empty_string_".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnNewListExpr) });
                grm.RegisterRule("Args1", "comma Expr Args1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 1, 2)) });
                grm.RegisterRule("Args1", "_empty_string_".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnNewListExpr) });
                grm.RegisterRule("Literal", "id".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnVarId) });
                grm.RegisterRule("Literal", "string".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnLiteralConstant) });
                grm.RegisterRule("Literal", "double".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnLiteralConstant) });
                grm.RegisterRule("Literal", "int".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnLiteralConstant) });

                grm.SetStartSymbol("Grammar");
                return grm;
            }

            private static object MakeSymbolStart(object[] objs)
            {
                GrammarCreator grm = (GrammarCreator)objs[1];
                RuleTriple triple = (RuleTriple)objs[0];
                grm.SetStartSymbol(triple.NonTerm);
                return grm;
            }
            private static object RegisterRuleInGrammar(object[] objs)
            {
                GrammarCreator grm = (GrammarCreator)objs[1];
                RuleTriple triple = (RuleTriple)objs[0];
                grm.RegisterRule(triple.NonTerm, triple.Symbols, triple.Expressions);
                return grm;
            }
            private static object ReturnGrammarCreator(object[] objs, AssembliesAccessWrapper wrapper, RuleExpressionEvaluator ExpressionEvaluator)
            {
                return new GrammarCreator(ExpressionEvaluator, wrapper);
            }
            private static object CreateRuleTriple(object[] objs)
            {
                var triple = new RuleTriple();
                triple.NonTerm = objs[0] as string;
                triple.Symbols = objs[2] as string[];
                var exprs = (objs[4] as List<object>);
                exprs.Reverse();
                triple.Expressions = (RuleExpressionFactory[])exprs.ToArray();
                return triple;
            }
            private static object CreateConstructorCalling(object[] objs, AssembliesAccessWrapper wrapper)
            {
                var reversed = objs[1] as List<string>;
                reversed.Reverse();
                var tname = ConcatSymbolsToGetName(reversed);
                var args = objs[3] as List<RuleExpressionFactory>;
                args.Reverse();
                return new ConstructorCallingCreator(wrapper, tname, args.ToArray());
            }
            private static object CreateStaticMethodCalling(object[] objs, AssembliesAccessWrapper wrapper)
            {
                var reversed = objs[1] as List<string>;
                reversed.Reverse();
                var mname = ConcatSymbolsToGetName(reversed);
                var args = objs[3] as List<RuleExpressionFactory>;
                args.Reverse();
                return new StaticMethodCallingCreator(wrapper, mname, args.ToArray());
            }
            private static object CreateInstanceMethodCalling(object[] objs, AssembliesAccessWrapper wrapper)
            {
                var reversed = objs[0] as List<string>;
                reversed.Reverse();
                var mname = ConcatSymbolsToGetName(reversed);
                var args = objs[3] as List<RuleExpressionFactory>;
                args.Reverse();
                var result = new InstanceMethodCallingCreator(wrapper, mname, args.ToArray());
                result.Context = objs[2] as RuleExpressionFactory;
                return result;
            }
            private static string ConcatSymbolsToGetName(List<string> strs)
            {
                StringBuilder res = new StringBuilder();
                for (int i = 0; i < strs.Count - 1; i++)
                    res.Append(strs[i] + ".");
                res.Append(strs[strs.Count - 1]);
                return res.ToString();
            }
            private static object ReturnLiteral(object[] objs)
            {
                return objs[0];
            }
            private static object ReturnNewListString(object[] objs)
            {
                return new List<string>();
            }
            private static object AppendAndReturnList(object[] objs, int elem, int list)
            {
                (objs[list] as List<object>).Add(objs[elem]);
                return objs[list];
            }
            private static object ReturnNewListExpr(object[] objs)
            {
                return new List<RuleExpressionFactory>();
            }
            private static object ReturnLiteralConstant(object[] objs)
            {
                return new ConstantCreator<object>(objs[0]);
            }
            private static object ReturnVarId(object[] objs)
            {
                return new VariableCreator((uint)objs[0]);
            }
        }
    }
}
