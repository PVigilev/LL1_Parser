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

    abstract class InvokableCreator : RuleExpressionFactory
    {
        public abstract void AddNamePart(string name);
        public abstract void AddArgument(RuleExpressionFactory arg);
    }
    class StaticMethodCallingCreator : InvokableCreator
    {
        AssembliesAccessWrapper Wrapper;
        StringBuilder FullMethodNameBuilder;
        List<RuleExpressionFactory> Args;
        public StaticMethodCallingCreator(AssembliesAccessWrapper w) { Wrapper = (w ?? throw new ArgumentNullException($"AseemblyAccessWrapper is null")); }

        public override void AddNamePart(string name)
        {
            FullMethodNameBuilder.Append(name ?? throw new ArgumentNullException($"Name of method or type is null"));
        }
        public override void AddArgument(RuleExpressionFactory arg)
        {
            Args.Add(arg ?? throw new ArgumentNullException($"RuleExpressionFactory argument is null"));
        }
        public StaticMethodCallingCreator(AssembliesAccessWrapper w, string mname, RuleExpressionFactory[] a)
        {
            Wrapper = w;
            FullMethodNameBuilder = new StringBuilder(mname);
            Args = new List<RuleExpressionFactory>(a);
        }
        public override RuleExpression Create()
        {
            var FullMethodName = FullMethodNameBuilder.ToString();
            if (!AssembliesAccessWrapper.CheckFormat(FullMethodName))
                throw new FormatException($"Full method name {FullMethodName} is in a wrong format");
            RuleExpression[] args = new RuleExpression[Args.Count];
            for (int i = 0; i < args.Length; i++)
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

    class ConstructorCallingCreator : InvokableCreator
    {
        AssembliesAccessWrapper Wrapper;
        StringBuilder FullTypeNameBuilder;
        List<RuleExpressionFactory> Args;
        public ConstructorCallingCreator(AssembliesAccessWrapper w) { Wrapper = (w ?? throw new ArgumentNullException($"AseemblyAccessWrapper is null")); }

        public override void AddNamePart(string name)
        {
            FullTypeNameBuilder.Append(name ?? throw new ArgumentNullException($"Name of method or type is null"));
        }
        public override void AddArgument(RuleExpressionFactory arg)
        {
            Args.Add(arg ?? throw new ArgumentNullException($"RuleExpressionFactory argument is null"));
        }
        public ConstructorCallingCreator(AssembliesAccessWrapper w, string mname, RuleExpressionFactory[] a)
        {
            Wrapper = w;
            FullTypeNameBuilder = new StringBuilder(mname);
            Args = new List<RuleExpressionFactory>(a);
        }

        public override RuleExpression Create()
        {
            RuleExpression[] args = new RuleExpression[Args.Count];
            var FullTypeName = FullTypeNameBuilder.ToString();
            if (!AssembliesAccessWrapper.CheckFormat(FullTypeName))
                throw new FormatException($"Full type name {FullTypeName} is not in the right format");
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Args[i].Create();
            }
            var type = Wrapper.FindType(FullTypeName);
            return new ConstructorCalling(type, args);
        }
    }

    class InstanceMethodCallingCreator : InvokableCreator
    {
        public RuleExpressionFactory Context
        {
            private get { return Context; }
            set
            {
                Context = value ?? throw new ArgumentNullException($"Context must be non-null");
            }
        }
        AssembliesAccessWrapper Wrapper;
        StringBuilder FullMethodNameBuilder;
        List<RuleExpressionFactory> Args;
        public InstanceMethodCallingCreator(AssembliesAccessWrapper w) { Wrapper = (w ?? throw new ArgumentNullException($"AseemblyAccessWrapper is null")); }

        public override void AddNamePart(string name)
        {
            FullMethodNameBuilder.Append(name ?? throw new ArgumentNullException($"Name of method or type is null"));
        }
        public override void AddArgument(RuleExpressionFactory arg)
        {
            Args.Add(arg ?? throw new ArgumentNullException($"RuleExpressionFactory argument is null"));
        }
        public InstanceMethodCallingCreator(AssembliesAccessWrapper w, string mname, RuleExpressionFactory[] a)
        {
            Wrapper = w;
            FullMethodNameBuilder = new StringBuilder(mname);
            Args = new List<RuleExpressionFactory>(a);
        }
        public override RuleExpression Create()
        {
            var context = Context.Create();
            var FullMethodName = FullMethodNameBuilder.ToString();
            if (!AssembliesAccessWrapper.CheckFormat(FullMethodName))
                throw new FormatException($"Full method name {FullMethodName} is in a wrong format");
            RuleExpression[] args = new RuleExpression[Args.Count];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Args[i].Create();
            }

            var lastDot = FullMethodName.LastIndexOf('.');
            var typename = FullMethodName.Substring(0, lastDot);
            var methodname = FullMethodName.Substring(lastDot + 1);
            var type = Wrapper.FindType(typename);
            return new InstanceMethodCalling(type, methodname, context, args);
        }
    }
}