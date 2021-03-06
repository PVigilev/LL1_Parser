﻿namespace LL1_Parser
{

    /// <summary>
    /// class for evaluation of RuleExpression.
    /// Used Visitor-template. Visitor part
    /// </summary>
    public abstract class RuleExpressionEvaluator
    {
        public abstract object[] ParsingResult { get; set; }
        public abstract object Evaluate(Constant c);
        public abstract object Evaluate(Variable v);
        public abstract object Evaluate(StaticMethodCalling mc);
        public abstract object Evaluate(InstanceMethodCalling imc);
        public abstract object Evaluate(ConstructorCalling cc);
    }

}
