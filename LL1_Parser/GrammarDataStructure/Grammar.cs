using LL1_Parser.Bootstrap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
#if DEBUG
    public
#endif
    sealed class Grammar : IEnumerable<KeyValuePair<NonTerminal, HashSet<Rule>>>
    {
        public static readonly Grammar SpecialGrammar = DefaultInitializer.CreateDefaultSpecialGrammar();
        public readonly NonTerminal StartSymbol;
        Dictionary<NonTerminal, HashSet<Rule>> Rules;
        RuleExpressionEvaluator Evaluator;

        public Grammar(NonTerminal start, Dictionary<NonTerminal, HashSet<Rule>> rules, RuleExpressionEvaluator ev)
        {
            if (rules == null)
                throw new ArgumentNullException($"Set of rules is null");
            if (rules.Count == 0)
                throw new ArgumentException("Set of rules is empty");
            if (ev == null)
                throw new ArgumentNullException("RuleExpressionEvaluator is null");
            Rules = rules;
            Evaluator = ev;
            if (!Rules.ContainsKey(start))
                throw new GrammarException($"Starting non-terminal symbol is not defined in the grammar");
            StartSymbol = start;
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

        static class DefaultInitializer
        {
            public class RuleTriple
            {
                public string NonTerm;
                public string[] Symbols;
                public RuleExpressionFactory[] Expressions;
            }

            /// <summary>
            /// Returns GrammarCreator instance such that after registration all terminals
            /// </summary>
            /// <param name="wrapper"></param>
            /// <param name="ExpressionEvaluator"></param>
            /// <returns></returns>
            public static Grammar CreateDefaultSpecialGrammar()
            {
                GrammarCreator grm = new GrammarCreator();
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
                grm.RegisterRule("Grammar1", "_empty_string_".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnGrammarCreator) });
                grm.RegisterRule("Rule", "symbol column Symbols OCB Exprs CCB".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(CreateRuleTriple) });
                grm.RegisterRule("Symbols", "symbol Symbols1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 1)) });
                grm.RegisterRule("Symbols1", "symbol Symbols1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 1)) });
                grm.RegisterRule("Symbols", "_empty_string_".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnNewListString) });
                grm.RegisterRule("Exprs", "Expr semicolumn Exprs1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 2)) });
                grm.RegisterRule("Exprs1", "Expr semicolumn Exprs1".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator((o) => AppendAndReturnList(o, 0, 2)) });
                grm.RegisterRule("Exprs", "_empty_string_".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(ReturnNewListExpr) });
                grm.RegisterRule("Expr", "new Name OB Args CB".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(CreateConstructorCalling) });
                grm.RegisterRule("Expr", "static Name OB Args CB".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(CreateStaticMethodCalling) });
                grm.RegisterRule("Expr", "Name OB Expr Args1 CB".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(CreateInstanceMethodCalling) });
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
                grm.Assemblies = new AssembliesAccessWrapper(System.Reflection.Assembly.GetExecutingAssembly());
                grm.ExpressionEvaluator = new REEvaluator();
                return grm.Create();
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
            private static object ReturnGrammarCreator(object[] objs)
            {
                return new GrammarCreator();
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
            private static object CreateConstructorCalling(object[] objs)
            {
                var reversed = objs[1] as List<string>;
                reversed.Reverse();
                var tname = ConcatSymbolsToGetName(reversed);
                var args = objs[3] as List<RuleExpressionFactory>;
                args.Reverse();
                return new ConstructorCallingCreator(tname, args.ToArray());
            }
            private static object CreateStaticMethodCalling(object[] objs)
            {
                var reversed = objs[1] as List<string>;
                reversed.Reverse();
                var mname = ConcatSymbolsToGetName(reversed);
                var args = objs[3] as List<RuleExpressionFactory>;
                args.Reverse();
                return new StaticMethodCallingCreator(mname, args.ToArray());
            }
            private static object CreateInstanceMethodCalling(object[] objs)
            {
                var reversed = objs[0] as List<string>;
                reversed.Reverse();
                var mname = ConcatSymbolsToGetName(reversed);
                var args = objs[3] as List<RuleExpressionFactory>;
                args.Reverse();
                var result = new InstanceMethodCallingCreator(mname, args.ToArray());
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
