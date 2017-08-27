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
            var virtuals = new Dictionary<MethodInfo, MethodInfo>();
            CollectFixtureMethods(type, x => Classify(x, virtuals));
			foreach(var item in virtuals.Values)
				classifier.Classify(new Invokable(item));
        }

        void CollectFixtureMethods(Type type, Action<MethodInfo> classify) {
            if(type == typeof(object))
                return;
            CollectFixtureMethods(type.BaseType, classify);
            GetMethods(type).ForEach(classify);
        }

        void Classify(MethodInfo method, Dictionary<MethodInfo, MethodInfo> virtuals) { 
            if(method.IsVirtual) {
				virtuals[method.GetBaseDefinition()] = method;
				return;
            }
            classifier.Classify(new Invokable(method));
        }

        MethodInfo[] GetMethods(Type type) {
            return type.GetMethods(OwnPublicMethods);
        }
    }
}
