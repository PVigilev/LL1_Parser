using System;
using System.Reflection;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("AssemblyWrapperTests")]
namespace LL1_Parser
{

    /// <summary>
    /// Wrapper around an array of assemblies for finding types and methods inside them.
    /// </summary>    
    class AssembliesAccessWrapper
    {
        Assembly[] assemblies;
        public AssembliesAccessWrapper(Assembly[] asms)
        {
            if (asms == null)
                throw new ArgumentNullException($"Array of assemblies is null");
            if (asms.Length == 0)
                throw new ArgumentException("Array of assemblies is empty");
            assemblies = asms;
        }
        public AssembliesAccessWrapper(Assembly asm)
        {
            if (asm == null)
                throw new ArgumentNullException($"Assembly object is null");
            assemblies = new Assembly[] { asm };
        }

        /// <summary>
        /// Finds type with specified name in all assemblies.
        /// Does not support generic types
        /// </summary>
        /// <param name="fullTypeName"> name of the type in the form: namespace1.namespace2. ... .typename1.typename2. ... .typenameN </param>
        /// <returns>Returns type with the specified name that from the fierst assembly where is type with this name</returns>
        public Type FindType(string fullTypeName)
        {
            if (fullTypeName == null)
                throw new ArgumentNullException("Typename is null");
            if (!CheckFormat(fullTypeName))
                throw new FormatException($"The string {fullTypeName} is in wrong format and does not represent any type");

            foreach (var asm in assemblies)
            {
                Type t = FindTypeInAssembly(fullTypeName, asm);
                if (t != null)
                    return t;
            }
            throw new TypeNotFoundException($"Type {fullTypeName} is not found in the passed assemblies");
        }

        /// <summary>
        /// Search the Type definition in the assembly asm wich are visible from this class
        /// </summary>
        /// <param name="correctFullTypeName"> have to be checked for the correctness name of type </param>
        /// <param name="asm"> assmebly object</param>
        /// <returns> Type object if it is found; null otherwise </returns>
        public static Type FindTypeInAssembly(string correctFullTypeName, Assembly asm)
        {
            Type result = null;

            int cur_dot = correctFullTypeName.IndexOf('.', 0);
            for (; cur_dot > 0 && cur_dot < correctFullTypeName.Length; cur_dot = correctFullTypeName.IndexOf('.', cur_dot + 1))
            {
                result = asm.GetType(correctFullTypeName.Substring(0, cur_dot));
                if (result != null)
                {
                    break;
                }
            }

            if (cur_dot < 0 && result == null)
            {
                return asm.GetType(correctFullTypeName);
            }

            // find nested type
            while (cur_dot > 0 && cur_dot < correctFullTypeName.Length)
            {
                string name_nested_type = null;
                int next_dot = correctFullTypeName.IndexOf('.', cur_dot + 1);
                if (next_dot < 0)
                {
                    name_nested_type = correctFullTypeName.Substring(cur_dot + 1);
                }
                else
                {
                    name_nested_type = correctFullTypeName.Substring(cur_dot + 1, next_dot - cur_dot - 1);
                }
                result = result.GetNestedType(name_nested_type);
                cur_dot = next_dot;
            }
            return result;
        }

        /// <summary>
        /// Check if the string "fullname" can be interpreted as a fullname of some type or method or enum-field
        /// </summary>
        /// <param name="fullname"></param>
        /// <returns></returns>
        public static bool CheckFormat(string fullname)
        {
            if (fullname == null)
                return false;
            if (fullname.Length == 0)
                return false;
            if (fullname[0] == '.' || char.IsDigit(fullname[0]))
                return false;

            bool firstSym = true;
            for (int i = 0; i < fullname.Length; i++)
            {
                if (firstSym)
                {
                    if (char.IsWhiteSpace(fullname[i]))
                        continue;
                    else if (char.IsLetter(fullname[i]) || fullname[i] == '_')
                    {
                        firstSym = false;
                        continue;
                    }
                    else return false;
                }
                else
                {
                    if (char.IsLetterOrDigit(fullname[i]) || fullname[i] == '_')
                        continue;
                    else if (fullname[i] == '.')
                    {
                        firstSym = true;
                        continue;
                    }
                    else return false;
                }
            }
            return !firstSym;
        }

        /// <summary>
        /// Finds method with the specified name
        /// </summary>
        /// <param name="fullMethodName"> typename1.typename2.typenameN.methodname</param>
        /// <param name="types">types of arguments. If not specified may occur AmbigousMethodnameException</param>
        /// <returns>MethodInfo object assotiated with the method with specified name</returns>
        public MethodInfo FindMethod(string fullMethodName, Type[] types = null)
        {
            if (fullMethodName == null)
                throw new ArgumentNullException("Name of a method passed to the function is null");
            if (!CheckFormat(fullMethodName))
                throw new FormatException($"The \"{fullMethodName}\" is in wrong format and can not represent a full method name");
            var lastDot = fullMethodName.LastIndexOf('.');
            string methodname = fullMethodName.Substring(lastDot + 1);
            Type t = FindType(fullMethodName.Substring(0, lastDot));
            MethodInfo method = null;

            if (types != null)
                method = t.GetMethod(methodname, types);
            else
                method = t.GetMethod(methodname);


            return method;
        }

        /// <summary>
        /// Finds a constructor for type with specified name.
        /// </summary>
        /// <param name="typename">Full name of the type</param>
        /// <param name="types">Finds constructor with types of arguments described by this argument</param>
        /// <returns>ConstructorInfo object assotiated with the constructor for this type</returns>
        public ConstructorInfo FindConstructor(string typename, Type[] types)
        {
            if (typename == null)
                throw new ArgumentNullException("Typename argument is null");
            if (types == null) types = new Type[0];
            Type type = FindType(typename);
            var constructor = type.GetConstructor(types);
            if (constructor != null)
                return constructor;
            throw new MethodNotFoundException($"Constructor for type {typename} with passed types was not found");
        }

        public static Type[] GetTypes(object[] objects)
        {
            if (objects == null)
                return null;
            Type[] types = new Type[objects.Length];
            for (int i = 0; i < objects.Length; i++)
                types[i] = (objects[i] == null ? typeof(object) : objects[i].GetType());
            return types;
        }

        /// <summary>
        /// Invoke method with specified name 
        /// </summary>
        /// <param name="FullMethodName"></param>
        /// <param name="context"></param>
        /// <param name="args"></param>
        /// <returns>result of the method invocation</returns>
        public object InvokeMethod(string FullMethodName, object context, object[] args)
        {
            var method = FindMethod(FullMethodName, GetTypes(args));
            return method.Invoke(context, args);
        }

        /// <summary>
        /// Invokes constructor of type "typename" with arguments args
        /// </summary>
        /// <param name="typename"></param>
        /// <param name="args"></param>
        /// <exception> Throws </exception>
        /// <returns></returns>
        public object InvokeConstructor(string typename, object[] args)
        {
            var constr = FindConstructor(typename, GetTypes(args));
            return constr.Invoke(args);
        }
    }
}
