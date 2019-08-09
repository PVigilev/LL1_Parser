using NUnit.Framework;
using System.Reflection;
using MFFParser;
using System;

namespace namespace1
{
    namespace namespace2
    {
        public class Type1
        {
            public class Type2
            {
                public class Type3
                {
                    public static int Method(int a, int b)
                    {
                        return 1;
                    }
                    public int Method(int a)
                    {
                        return 2;
                    }
                    public int Method()
                    {
                        return 3;
                    }
                    public int Method<T>(T t)
                    {
                        return 4;
                    }
                }
            }
            public class Type4
            {
                public class Type5
                {
                    private class Type6 { }
                    static Type5()
                    {
                        Type6 a = new Type6();
                    }
                    public Type5() { }
                    public Type5(int a) { }
                    public Type5(string a) { }
                }
            }
        }
    }
}

namespace InvocationUsingReflectionTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            namespace1.namespace2.Type1.Type2.Type3 a = new namespace1.namespace2.Type1.Type2.Type3();
            //namespace1.namespace2.Type1.Type4.Type5 b = new namespace1.namespace2.Type1.Type4.Type5();
        }

        [Test]
        // Finding namespace1.namespace2.Type1.Type2.Type3.Method
        public void InvokingMethodTest()
        {
            string fullMethodName = "namespace1.namespace2.Type1.Type2.Type3.Method";
            object context = new namespace1.namespace2.Type1.Type2.Type3();
            Assembly asm = Assembly.GetExecutingAssembly();
            AssembliesAccessWrapper aaw = new AssembliesAccessWrapper(new Assembly[] { asm });

            // Invoke 2-arg method 
            CheckInvokationResultCorrectness(aaw.InvokeMethod(fullMethodName, context, new object[] { 1, 2 }), 1, "problem with invocation of the 2-arg method", "Wrong type of the result of invocation");

            // Invoke 2-arg static method as a static method
            CheckInvokationResultCorrectness(aaw.InvokeMethod(fullMethodName, null, new object[] { 1, 2 }), 1, "problem with invocation of the 2-arg method", "Wrong type of the result of invocation");
            
            // Invoke 1-arg method
            CheckInvokationResultCorrectness(aaw.InvokeMethod(fullMethodName, context, new object[] { 1 }), 2, "problem with invocation of the 1-arg method", "Wrong type of the result of invocation");

            // Invoke 0-arg method
            CheckInvokationResultCorrectness(aaw.InvokeMethod(fullMethodName, context, new object[] { }), 3, "problem with invocation of the 0-arg method", "Wrong type of the result of invocation");

            // Invoke 1-arg generic method
            //CheckInvokationResultCorrectness(aaw.InvokeMethod(fullMethodName, context, new object[] { "abc" }), 4, "problem with invocation of the 0-arg method", "Wrong type of the result of invocation");
        }


        [Test]
        public void InvokeConstructorTest()
        {
            string typeName = "namespace1.namespace2.Type1.Type4.Type5";
            AssembliesAccessWrapper aaw = new AssembliesAccessWrapper(new Assembly[] { Assembly.GetExecutingAssembly() });

            object res1 = aaw.InvokeConstructor(typeName, null);

            if (!(res1 is namespace1.namespace2.Type1.Type4.Type5))
                Assert.Fail("null argument as a empty set of args failed");

            object res2 = aaw.InvokeConstructor(typeName, new object[] {});
            if (!(res2 is namespace1.namespace2.Type1.Type4.Type5))
                Assert.Fail("empty array of args failed");

            object res3 = aaw.InvokeConstructor(typeName, new object[] { 1 });
            if (!(res3 is namespace1.namespace2.Type1.Type4.Type5))
                Assert.Fail("single int-arg failed");

            object res4 = aaw.InvokeConstructor(typeName, new object[] { "str" });
            if (!(res4 is namespace1.namespace2.Type1.Type4.Type5))
                Assert.Fail("single string-arg failed");
        }

        void CheckInvokationResultCorrectness<T>(object result, T expect, string differentValueMessage, string differentTypeMessage) where T : IEquatable<T>
        {
            if (result is T)
            {
                if (!((T)result).Equals(expect))
                    Assert.Fail(differentValueMessage);
            }
            else Assert.Fail(differentTypeMessage);
        }
      
        [Test]        
        public void CheckNameTest()
        {
            Assert.IsTrue(AssembliesAccessWrapper.CheckFormat("ArithmeticsTest.BinaryOperation.set_Left"));
            string[] inputs = new string[] { "a.b.c", "a..b", "aa2.3.c", ".b.c", "", "a.b." };
            if (!AssembliesAccessWrapper.CheckFormat(inputs[0]))
                Assert.Fail("Does not accept correct name");
            for(int i = 1; i < inputs.Length; i++)
            {
                if (AssembliesAccessWrapper.CheckFormat(inputs[i]))
                    Assert.Fail("Accept wrong format");
            }
        }


        [Test]
        public void FindTypeTest()
        {
            string name = "namespace1.namespace2.Type1.Type4";
            Assert.AreEqual(typeof(namespace1.namespace2.Type1.Type4), AssembliesAccessWrapper.FindTypeInAssembly("namespace1.namespace2.Type1.Type4", typeof(namespace1.namespace2.Type1.Type4).Assembly));
            Assert.AreEqual(typeof(namespace1.namespace2.Type1.Type2.Type3), AssembliesAccessWrapper.FindTypeInAssembly("namespace1.namespace2.Type1.Type2.Type3", typeof(namespace1.namespace2.Type1.Type2.Type3).Assembly));
            Assert.AreEqual(typeof(namespace1.namespace2.Type1), AssembliesAccessWrapper.FindTypeInAssembly("namespace1.namespace2.Type1", typeof(namespace1.namespace2.Type1).Assembly));
        }
    }
}