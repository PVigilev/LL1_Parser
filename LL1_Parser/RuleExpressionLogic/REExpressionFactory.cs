using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    /// <summary>
    /// Factory-method pattern for creating RuleExpressions
    /// </summary>
    abstract class RuleExpressionFactory
    {
        public abstract RuleExpression Create();
    }
    class VariableCreator : RuleExpressionFactory
    {
        uint Id;
        public VariableCreator(uint id) { Id = id; }
        public override RuleExpression Create()
        {
            return new Variable(Id);
        }
    }
    class ConstantCreator<T> : RuleExpressionFactory
    {
        T Value;
        public ConstantCreator(T val) => Value = val;
        public override RuleExpression Create()
        {
            return new Constant(Value);
        }
    }
    class StaticMethodCallingCreator : RuleExpressionFactory
    {
        AssembliesAccessWrapper Wrapper;
        string FullMethodName;
        RuleExpressionFactory[] Args;
        public StaticMethodCallingCreator(AssembliesAccessWrapper w, string mname, RuleExpressionFactory[] a)
        {
            Wrapper = w;
            FullMethodName = mname;
            Args = a;
        }
        public override RuleExpression Create()
        {
            RuleExpression[] args = new RuleExpression[Args.Length];
            for(int i = 0; i < args.Length; i++)
            {
                args[i] = Args[i].Create();
            }

            var lastDot = FullMethodName.LastIndexOf('.');
            var typename = FullMethodName.Substring(0, lastDot);
            var methodname = FullMethodName.Substring(lastDot + 1);
            var type = Wrapper.FindType(typename);
            return new StaticMethodCalling(type, methodname, args);
        }
    }

    class ConstructorCallingCreator : RuleExpressionFactory
    {
        AssembliesAccessWrapper Wrapper;
        string FullTypeName;
        RuleExpressionFactory[] Args;
        public ConstructorCallingCreator(AssembliesAccessWrapper w, string tname, RuleExpressionFactory[] a)
        {
            Wrapper = w;
            FullTypeName = tname;
            Args = a;
        }

        public override RuleExpression Create()
        {
            RuleExpression[] args = new RuleExpression[Args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Args[i].Create();
            }
            var type = Wrapper.FindType(FullTypeName);
            return new ConstructorCalling(type, args);
        }
    }

    class InstanceMethodCallingCreator : RuleExpressionFactory
    {
        RuleExpressionFactory context;
        AssembliesAccessWrapper Wrapper;
        string FullMethodName;
        RuleExpressionFactory[] Args;

        public InstanceMethodCallingCreator(AssembliesAccessWrapper wrapper, string fullMethodName, RuleExpressionFactory[] args, RuleExpressionFactory context)
        {
            Wrapper = wrapper;
            FullMethodName = fullMethodName;
            Args = args;
            this.context = context;
        }

        public override RuleExpression Create()
        {
            RuleExpression cntxt = context.Create();
            RuleExpression[] args = new RuleExpression[Args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Args[i].Create();
            }

            var lastDot = FullMethodName.LastIndexOf('.');
            var typename = FullMethodName.Substring(0, lastDot);
            var methodname = FullMethodName.Substring(lastDot + 1);
            var type = Wrapper.FindType(typename);
            return new InstanceMethodCalling(type, methodname, cntxt, args);
        }
    }
}
