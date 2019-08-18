using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    /// <summary>
    /// Represents a single expression that will be evaluated after success of parsing
    /// Used Vsitor template. This part is acceptor
    /// </summary>
    public abstract class RuleExpression
    {
        public abstract object Evaluate(RuleExpressionEvaluator evaluator);
    }


    /// <summary>
    /// In the rule of a grammar after a success parsing the number of the parsed symbol (nonterminal or termianl with value)
    /// </summary>
    public class Variable : RuleExpression
    {
        public readonly uint NumberOfVariable;
        public Variable(uint n)
        {
            if (n < 0)
                throw new ArgumentException("Number of variable must be greater or equal than zero");
            NumberOfVariable = n;
        }
        public override object Evaluate(RuleExpressionEvaluator evaluator)
        {
            return evaluator.Evaluate(this);
        }
    }

    public class Constant : RuleExpression
    {
        public readonly object Value;
        public Constant(object obj)
        {
            Value = obj;
        }
        public override object Evaluate(RuleExpressionEvaluator evaluator)
        {
            return evaluator.Evaluate(this);
        }
    }


    public class StaticMethodCalling : RuleExpression
    {
        public Type Type { get; }
        public string MethodName { get; }
        public RuleExpression[] Args { get; }
        public StaticMethodCalling(Type t, string mname, RuleExpression[] a)
        {
            Type = t;
            MethodName = mname;
            Args = a;            
        }

        public override object Evaluate(RuleExpressionEvaluator evaluator)
        {
            return evaluator.Evaluate(this);
        }
    }


    public class InstanceMethodCalling : StaticMethodCalling
    {
        public RuleExpression Context { get; }
        public InstanceMethodCalling(Type t, string mname, RuleExpression cntxt, RuleExpression[] a) : base(t, mname, a)
        {
            Context = cntxt;
        }
        public override object Evaluate(RuleExpressionEvaluator evaluator)
        {
            return evaluator.Evaluate(this);
        }
    }


    public class ConstructorCalling : RuleExpression
    {
        public Type Type { get; }
        public RuleExpression[] Args { get; }
        public ConstructorCalling(Type t, RuleExpression[] rules)
        {
            Type = t;
            Args = rules;
        }
        public override object Evaluate(RuleExpressionEvaluator evaluator)
        {
            return evaluator.Evaluate(this);
        }
    }
}
