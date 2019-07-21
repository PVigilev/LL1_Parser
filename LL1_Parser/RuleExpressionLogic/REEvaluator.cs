using System;

namespace LL1_Parser
{
    /**/


    class REEvaluator : RuleExpressionEvaluator
    {
        public override object[] ParsingResult { get; set; }
    
        public override object Evaluate(Constant c)
        {
            return c.Value;
        }

        public override object Evaluate(Variable v)
        {
            if (v.NumberOfVariable >= ParsingResult.Length)
                throw new IndexOutOfRangeException($"The rule has {ParsingResult.Length} symbols, but the variable ${v.NumberOfVariable} is greater than rules size");
            return ParsingResult[v.NumberOfVariable];
        }

        public override object Evaluate(StaticMethodCalling mc)
        {
            object[] args;
            Type[] types;
            args = EvaluateExpressions(mc.Args, out types);
            var method= mc.Type.GetMethod(mc.MethodName, types);
            if (method == null)
                throw new MethodNotFoundException($"Static method {mc.Type.FullName}.{mc.MethodName} with passed argument types was not found");
            if (!method.IsStatic)
                throw new MethodNotFoundException($"Static method {mc.Type.FullName}.{mc.MethodName} with passed argument types is not static");
            if(!method.IsPublic)
                throw new MethodNotFoundException($"Static method {mc.Type.FullName}.{mc.MethodName} with passed argument types is not public");
            return method.Invoke(null, args);
        }

        public override object Evaluate(InstanceMethodCalling imc)
        {
            object context = imc.Context.Evaluate(this);
            Type contextType = context.GetType();
            if (!contextType.IsAssignableFrom(imc.Type))
                throw new ParserTypeErrorException($"Type {contextType.FullName} is not assignable from {imc.Type.FullName}, so the method {imc.MethodName} can not be invoked with context of type {contextType.FullName}");
            Type[] argTypes;
            object[] args = EvaluateExpressions(imc.Args, out argTypes);
            var method = imc.Type.GetMethod(imc.MethodName, argTypes);
            if (method == null)
                throw new MethodNotFoundException($"Instance method {imc.Type.FullName}.{imc.MethodName} with passed argument types was not found");
            if (method.IsStatic)
                throw new MethodNotFoundException($"Instance method {imc.Type.FullName}.{imc.MethodName} with passed argument types was not found");
            if (!method.IsPublic)
                throw new MethodNotFoundException($"Instance method {imc.Type.FullName}.{imc.MethodName} with passed argument types is not public");
            return method.Invoke(context, args);
        }

        public override object Evaluate(ConstructorCalling cc)
        {
            object[] args;
            Type[] types;
            args = EvaluateExpressions(cc.Args, out types);
            var constructor = cc.Type.GetConstructor(types);
            if(constructor == null)
                throw new MethodNotFoundException($"Constructor for type {cc.Type.FullName} with passed types was not found");
            return constructor.Invoke(args);
        }

        private object[] EvaluateExpressions(RuleExpression[] expressions, out Type[] types)
        {
            object[] res = new object[expressions.Length];
            types = new Type[res.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = expressions[i].Evaluate(this);
                types[i] = res[i].GetType();
            }
            return res;
        }

        
    }

}
