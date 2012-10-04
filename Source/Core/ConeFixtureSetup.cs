using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public class ConeFixtureSetup
    {
        const BindingFlags OwnPublicMethods = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        readonly IMethodClassifier classifier;

        public ConeFixtureSetup(
			IMethodClassifier classifier) {
            this.classifier = classifier;
        }

        public void CollectFixtureMethods(Type type) {
            if(type == typeof(object))
                return;
            var seenVirtuals = new HashSet<MethodInfo>();
            CollectFixtureMethods(type, x => Classify(x, seenVirtuals));
        }

        void CollectFixtureMethods(Type type, Action<MethodInfo> classify) {
            if(type == typeof(object))
                return;
            CollectFixtureMethods(type.BaseType, classify);
            GetMethods(type).ForEach(classify);
        }

        void Classify(MethodInfo method, HashSet<MethodInfo> seenVirtuals) { 
            if(method.IsVirtual) {
                if(seenVirtuals.Contains(method.GetBaseDefinition()))
                    return;
                seenVirtuals.Add(method);
            }
            classifier.Classify(method); 
        }

        MethodInfo[] GetMethods(Type type) {
            return type.GetMethods(OwnPublicMethods);
        }
    }
}
