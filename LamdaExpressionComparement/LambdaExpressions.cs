using System;

namespace Lambda
{
    public abstract class LExprBasic
    {
    }
    /// <summary>
    /// single variable
    /// </summary>
    public class LVar :LExprBasic
    {
        public string Value { get; }
        public LVar(string val)
        {
            Value = val ?? throw new ArgumentNullException();
        }
        public override string ToString()
        {
            return Value;
        }
    }

    /// <summary>
    /// Application of two lambda expressions
    /// </summary>
    public class LApplication : LExprBasic
    {
        public LExprBasic What { get; }
        public LExprBasic To { get; }

        public LApplication(LExprBasic w, LExprBasic t)
        {
            What = w ?? throw new ArgumentNullException();
            To = t ?? throw new ArgumentNullException();
        }
        public override string ToString()
        {
            return $"({What.ToString()} {To.ToString()})";
        }
    }

    /// <summary>
    /// Lambda-term
    /// </summary>
    public class LAbstraction : LExprBasic
    {
        public LVar Variable { get; }
        public LExprBasic Expression { get; }
        public LAbstraction(LVar var, LExprBasic expr)
        {
            Variable = var ?? throw new ArgumentNullException();
            Expression = expr ?? throw new ArgumentNullException();
        }
        public override string ToString()
        {
            return $"(\\{Variable}.{Expression.ToString()})";
        }
    }
}
