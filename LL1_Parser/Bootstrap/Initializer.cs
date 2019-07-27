using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser.Bootstrap
{
    /**
     * *Grammar: Rule Grammar1   // register rule $0 in $1; $1
     * Grammar1: Rule Grammar1   // register $0 in $1; $1
     *         | empty_string    // create new GrammarCreator
     * Rule    : symbol : Symbols { Exprs } // create new rule ($0, $2, reverse $4)
     * Symbols : symbol Symbols1 // append $0 into $1; $1
     * Symbols1: symbol Symbols1 // append $0 into $1; $1
     *         | empty_string    // create new List<Symbol>
     * Exprs   : Expr ; Exprs1   // append $0 into $1; $1
     * Exprs1  : Expr ; Exprs1   // append $0 into $1; $1 
     *         | empty_string    // create new List<Expr>
     * Expr    : new Name ( Args )    // create ConstructorCallingCreator(concat(reverse($1)), reverse $3)
     *         | static Name ( Args ) // create StaticMethodCallingCreator(concat(reverse($1)), reverse $3)
     *         | Name ( Expr, Args )  // create InstanceMethodCallingCreator(concat(reverse($0), $2, reverse $4)
     *         | Literal              // $0
     * Name    : symbol . Name1       // append $1 to $0
     * Name1   : symbol . Name1       // append $1 to $0
     *         | empty_string         // create new List<string>
     * Args    : Expr Args1           // append($1, $0); $1;
     *         | empty_string         // create List<Expr>
     * Args1   : , Expr Args1         // append($2, $1); $2;        
     *         | empty_string         // create new List<Expr>
     * Literal : id                   // $0
     *         | string               // $0
     *         | double               // $0
     *         | int                  // $0
     */
#if DEBUG
    public
#else
    internal
#endif
    static class DefaultInitializer
    {

        public static readonly AssembliesAccessWrapper CurrentWrapper;
        public static Grammar GrammarForGrammarFile { get; private set; }

        static DefaultInitializer()
        {
            GrammarForGrammarFile = null;
            CurrentWrapper = new AssembliesAccessWrapper(System.Reflection.Assembly.GetExecutingAssembly());
        }

        public static void Init()
        {
            GrammarForGrammarFile = CreateDefaultGrammarCreator().Create();
        }
        public class RuleTriple
        {
            public string NonTerm;
            public string[] Symbols;
            public RuleExpressionFactory[] Expressions;
        }

#if DEBUG
        public
#endif
        static GrammarCreator CreateDefaultGrammarCreator()
        {
            GrammarCreator grm = new GrammarCreator(new REEvaluator());
            foreach(var pair in SimpleToken.Instances)
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
            grm.RegisterRule("Expr", "newKeyWord Name OB Args CB".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(CreateConstructorCalling) });
            grm.RegisterRule("Expr", "staticKeyWord Name OB Args CB".Split(' '), new DelegateRuleExpressionCreator[] { new DelegateRuleExpressionCreator(CreateStaticMethodCalling) });
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
        private static object ReturnGrammarCreator(object[] objs)
        {
            return CreateDefaultGrammarCreator();
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
            return new ConstructorCallingCreator(CurrentWrapper, tname, args.ToArray());
        }
        private static object CreateStaticMethodCalling(object[] objs)
        {
            var reversed = objs[1] as List<string>;
            reversed.Reverse();
            var mname = ConcatSymbolsToGetName(reversed);
            var args = objs[3] as List<RuleExpressionFactory>;
            args.Reverse();
            return new StaticMethodCallingCreator(CurrentWrapper, mname, args.ToArray());
        }
        private static object CreateInstanceMethodCalling(object[] objs)
        {
            var reversed = objs[0] as List<string>;
            reversed.Reverse();
            var mname = ConcatSymbolsToGetName(reversed);
            var args = objs[3] as List<RuleExpressionFactory>;
            args.Reverse();
            var result = new InstanceMethodCallingCreator(CurrentWrapper, mname, args.ToArray());
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

        //private object AppendAndReturnList(object[] objs)
        /// 
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
