using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArithmeticExpr
{
    public abstract class Expr
    {
        public abstract int Eval();
    }
    public abstract class BinaryOperation : Expr
    {
        public Expr Left { get; set; }
        public Expr Right { get; set; }
        public BinaryOperation() { }
        public BinaryOperation(Expr left, Expr right)
        {
            Left = left;
            Right = right;
        }
    }
    public class Addition : BinaryOperation
    {
        public Addition() :base() { }
        public Addition(Expr left, Expr right) : base(left, right)
        {
        }

        public override int Eval()
        {
            return Left.Eval() + Right.Eval();
        }
    }

    public class Substract : BinaryOperation
    {
        public Substract()
        {
        }

        public Substract(Expr left, Expr right) : base(left, right)
        {
        }

        public override int Eval()
        {
            return Left.Eval() - Right.Eval();
        }
    }
    public class Multiply : BinaryOperation
    {
        public Multiply()
        {
        }

        public Multiply(Expr left, Expr right) : base(left, right)
        {
        }

        public override int Eval()
        {
            return Left.Eval() * Right.Eval();
        }
    }
    public class Divide : BinaryOperation
    {
        public Divide()
        {
        }

        public Divide(Expr left, Expr right) : base(left, right)
        {
        }

        public override int Eval()
        {
            var r = Right.Eval();
            return Left.Eval() / (r == 0 ? throw new DivideByZeroException() : r);
        }
    }
    public class Constant : Expr
    {
        int Value;
        public Constant(int val) { Value = val; }
        public override int Eval()
        {
            return Value;
        }
    }
}
