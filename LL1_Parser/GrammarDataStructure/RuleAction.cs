using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{

    /// <summary>
    /// Represents expression that have to be evaluated after succes parsing
    /// </summary>
    public class RuleAction
    {
        RuleExpression[] expressions;

        public RuleAction(RuleExpression[] expressions)
        {
            if (expressions == null)
                throw new ArgumentNullException("Sequence of rule-expressions is null");
            if (expressions.Length == 0)
                throw new ArgumentException("Sequence of rule-expression must be non-empty");
            this.expressions = expressions;
        }
        /// <summary>
        /// Evaluate the sequence of expression using the given result of successed parsing.
        /// </summary>
        /// <param name="ParsingResult"> results of evaluating of each symbol in the rule after success parsing</param>
        /// <returns></returns>
        public object Evaluate(object[] ParsingResult, RuleExpressionEvaluator evaluator)
        {
            if (evaluator == null)
                throw new ArgumentNullException("RuleExpressionEvaluator is null");

            if (ParsingResult == null)
                throw new ArgumentNullException("Parsing result is null");
            //if (ParsingResult.Length == 0)
                //throw new ArgumentException("Parsing result is empty");
            evaluator.ParsingResult = ParsingResult;
            for(int i = 0; i < expressions.Length - 1; i++)
            {                
                expressions[i].Evaluate(evaluator);
            }
            var res = expressions[expressions.Length - 1].Evaluate(evaluator);
            if (res == null || res.GetType() == typeof(void))
                throw new ParserReturnValueException("The last expression has to return value");
            evaluator.ParsingResult = null;
            return res;
        }
    }
    

    
}
