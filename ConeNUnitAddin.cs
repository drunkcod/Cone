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

    [NUnitAddin(Name= "Cone")]
    public class ConeNUnitAddin : IAddin, ISuiteBuilder
    {
        bool IAddin.Install(IExtensionHost host) {
            var suiteBuilders = host.GetExtensionPoint("SuiteBuilders");
            if (suiteBuilders == null)
                return false;
            suiteBuilders.Install(this);
            Verify.ExpectationFailed = AssertionFailed;
            return true;
        }

        Test ISuiteBuilder.BuildFrom(Type type) {
            return ConeSuite.For(type);
        }

        bool ISuiteBuilder.CanBuildFrom(Type type) {
            return type.Has<DescribeAttribute>();
        }

        static void AssertionFailed(string message) {
            throw new AssertionException(message);
        }
    }
}
