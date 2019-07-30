using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    /// <summary>
    /// Factory-method pattern for creating RuleExpressions
    /// </summary>
#if DEBUG
    public
#endif
    abstract class RuleExpressionFactory
    {
        public abstract RuleExpression Create(AssembliesAccessWrapper assemblies);
    }
#if DEBUG
    public
#endif
    class VariableCreator : RuleExpressionFactory
    {
        uint Id;
        public VariableCreator(uint id) { Id = id; }
        public override RuleExpression Create(AssembliesAccessWrapper assemblies)
        {
            return new Variable(Id);
        }
    }
#if DEBUG
    public
#endif
    class ConstantCreator<T> : RuleExpressionFactory
    {
        T Value;
        public ConstantCreator(T val) => Value = val;
        public override RuleExpression Create(AssembliesAccessWrapper assemblies)
        {
            return new Constant(Value);
        }
    }
#if DEBUG
    public
#endif
    abstract class InvokableCreator : RuleExpressionFactory
    {
        protected StringBuilder FullNameBuilder;
        protected List<RuleExpressionFactory> Args;
        public abstract void AddNamePart(string name);
        public abstract void AddArgument(RuleExpressionFactory arg);
    }
#if DEBUG
    public
#endif
    class StaticMethodCallingCreator : InvokableCreator
    {
        public override void AddNamePart(string name)
        {
            FullNameBuilder.Append(name ?? throw new ArgumentNullException($"Name of method or type is null"));
        }
        public override void AddArgument(RuleExpressionFactory arg)
        {
            Args.Add(arg ?? throw new ArgumentNullException($"RuleExpressionFactory argument is null"));
        }
        public StaticMethodCallingCreator(string mname, RuleExpressionFactory[] a)
        {
            FullNameBuilder = new StringBuilder(mname);
            Args = new List<RuleExpressionFactory>(a);
        }
        public override RuleExpression Create(AssembliesAccessWrapper assemblies)
        {
            var FullMethodName = FullNameBuilder.ToString();
            if (!AssembliesAccessWrapper.CheckFormat(FullMethodName))
                throw new FormatException($"Full method name {FullMethodName} is in a wrong format");
            RuleExpression[] args = new RuleExpression[Args.Count];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Args[i].Create(assemblies);
            }

            var lastDot = FullMethodName.LastIndexOf('.');
            var typename = FullMethodName.Substring(0, lastDot);
            var methodname = FullMethodName.Substring(lastDot + 1);
            var type = assemblies.FindType(typename);
            return new StaticMethodCalling(type, methodname, args);
        }
    }
#if DEBUG
    public
#endif
    class ConstructorCallingCreator : InvokableCreator
    {
        public override void AddNamePart(string name)
        {
            FullNameBuilder.Append(name ?? throw new ArgumentNullException($"Name of method or type is null"));
        }
        public override void AddArgument(RuleExpressionFactory arg)
        {
            Args.Add(arg ?? throw new ArgumentNullException($"RuleExpressionFactory argument is null"));
        }
        public ConstructorCallingCreator(string mname, RuleExpressionFactory[] a)
        {
            FullNameBuilder = new StringBuilder(mname);
            Args = new List<RuleExpressionFactory>(a);
        }

        public override RuleExpression Create(AssembliesAccessWrapper assemblies)
        {
            RuleExpression[] args = new RuleExpression[Args.Count];
            var FullTypeName = FullNameBuilder.ToString();
            if (!AssembliesAccessWrapper.CheckFormat(FullTypeName))
                throw new FormatException($"Full type name {FullTypeName} is not in the right format");
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Args[i].Create(assemblies);
            }
            var type = assemblies.FindType(FullTypeName);
            return new ConstructorCalling(type, args);
        }
    }

#if DEBUG
    public
#endif
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
        public override void AddNamePart(string name)
        {
            if ((name ?? throw new ArgumentNullException($"Name of method or type is null")).Length == 0)
                throw new ParserErrorException("Name of type or method must be non-empty");
            FullNameBuilder.Append(name ?? throw new ArgumentNullException($"Name of method or type is null"));
        }
        public override void AddArgument(RuleExpressionFactory arg)
        {
            Args.Add(arg ?? throw new ArgumentNullException($"RuleExpressionFactory argument is null"));
        }
        public InstanceMethodCallingCreator(string mname, RuleExpressionFactory[] a)
        {
            FullNameBuilder = new StringBuilder(mname);
            Args = new List<RuleExpressionFactory>(a);
        }
        public override RuleExpression Create(AssembliesAccessWrapper assemblies)
        {
            var context = Context.Create(assemblies);
            var FullMethodName = FullNameBuilder.ToString();
            if (!AssembliesAccessWrapper.CheckFormat(FullMethodName))
                throw new FormatException($"Full method name {FullMethodName} is in a wrong format");
            RuleExpression[] args = new RuleExpression[Args.Count];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Args[i].Create(assemblies);
            }

            var lastDot = FullMethodName.LastIndexOf('.');
            var typename = FullMethodName.Substring(0, lastDot);
            var methodname = FullMethodName.Substring(lastDot + 1);
            var type = assemblies.FindType(typename);
            return new InstanceMethodCalling(type, methodname, context, args);
        }
    }
}