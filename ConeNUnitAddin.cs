using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Core;
using NUnit.Core.Extensibility;
using NUnit.Framework;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Core.Builders;

namespace Cone
{
    static class TypeExtensions
    {
        public static bool Has<T>(this Type type) {
            return type.GetCustomAttributes(typeof(T), true).Length == 1;
        }

        public static bool TryGetAttribute<T>(this Type type, out T value) {
            var attributes = type.GetCustomAttributes(typeof(T), true);
            if (attributes.Length == 1) {
                value = (T)attributes[0];
                return true;
            }
            value = default(T);
            return false;
        }
    }

    class ConeSuite : TestSuite
    {
        static readonly Regex normalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);
        readonly Type type;

        public static TestSuite For(Type type) {
            var suite = new ConeSuite(type);
            foreach (var item in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                if (item.DeclaringType == type) {
                    var parms = new ParameterSet {
                        TestName = NameFor(item)
                    };
                    suite.Add(NUnitTestCaseBuilder.BuildSingleTestMethod(item, suite, parms));
                }
            foreach (var context in type.GetNestedTypes())
                if (context.Has<ContextAttribute>())
                    suite.Add(For(context));
            return suite;
        }

        ConeSuite(Type type) : base(ParentFor(type), NameFor(type)){
            this.type = type;
        }

        static string NameFor(MethodInfo method) {
            return normalizeNamePattern.Replace(method.Name, " ");
        }

        static string ParentFor(Type type) {
            return DescriptionOf(type).DescribedType.Namespace;
        }

        static string NameFor(Type type) {
            DescribeAttribute desc;
            if (type.TryGetAttribute<DescribeAttribute>(out desc)) {
                if (string.IsNullOrEmpty(desc.Context))
                    return desc.DescribedType.Name;
                return desc.DescribedType.Name + " - " + desc.Context;
            }
            ContextAttribute context;
            type.TryGetAttribute<ContextAttribute>(out context);
            return context.Context;
        }

        static DescribeAttribute DescriptionOf(Type type) {
            DescribeAttribute desc;
            if(!type.TryGetAttribute<DescribeAttribute>(out desc))
                type.DeclaringType.TryGetAttribute<DescribeAttribute>(out desc);
            return desc;
        }
    }

    [NUnitAddin(Name= "Cone")]
    public class ConeNUnitAddin : IAddin, ISuiteBuilder
    {
        bool IAddin.Install(IExtensionHost host) {
            var suiteBuilders = host.GetExtensionPoint("SuiteBuilders");
            if (suiteBuilders == null)
                return false;
            suiteBuilders.Install(this);
            Verify.ExpectationFailed = message => { throw new AssertionException(message); };
            return true;
        }

        Test ISuiteBuilder.BuildFrom(Type type) {
            return ConeSuite.For(type);
        }

        bool ISuiteBuilder.CanBuildFrom(Type type) {
            return type.Has<DescribeAttribute>();
        }
    }
}
