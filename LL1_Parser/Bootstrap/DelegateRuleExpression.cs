using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser.Bootstrap
{
#if DEBUG
    public
#endif
    class DelegateRuleExpression : RuleExpression
    {
        public readonly Func<object[], object> Expression;
        public DelegateRuleExpression(Func<object[], object> expr)
        {
            Expression = expr ?? throw new ArgumentNullException($"Expression-argument in null");
        }
        public override object Evaluate(RuleExpressionEvaluator evaluator)
        {
            return Expression(evaluator.ParsingResult);
        }
    }
}
