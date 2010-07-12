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
    class ConeSuite : TestSuite
    {
        static readonly Regex normalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);
        readonly Type type;

        public static TestSuite For(Type type) {
            var suite = new ConeSuite(type);
            foreach (var item in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                if (item.DeclaringType == type) {
                    var parms = new ParameterSet();
                    parms.TestName = NameFor(item);
                    suite.Add(NUnitTestCaseBuilder.BuildSingleTestMethod(item, suite, parms));
                }
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
            var desc = DescriptionOf(type);
            if (string.IsNullOrEmpty(desc.Context))
                return desc.DescribedType.Name;
            return desc.DescribedType.Name + " - " + desc.Context;
        }

        static DescribeAttribute DescriptionOf(Type type) {
            return (DescribeAttribute)type.GetCustomAttributes(typeof(DescribeAttribute), true)[0];
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
            return type.GetCustomAttributes(typeof(DescribeAttribute), true).Length == 1;
        }
    }
}
