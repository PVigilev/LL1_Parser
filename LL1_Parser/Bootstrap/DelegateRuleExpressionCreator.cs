using System;
using System.Collections.Generic;
using System.Text;

namespace MFFParser.Bootstrap
{
#if DEBUG
    public
#endif
    class DelegateRuleExpressionCreator : RuleExpressionFactory
    {
        public Func<object[], object> Expression;
        public DelegateRuleExpressionCreator(Func<object[], object> expr)
        {
            Expression = expr;
        }
        public override RuleExpression Create(AssembliesAccessWrapper asms)
        {
            return new DelegateRuleExpression(Expression);
        }
    }
}
