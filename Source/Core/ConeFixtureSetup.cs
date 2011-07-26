using System;
using System.Reflection;

namespace Cone.Core
{
    public class ConeFixtureSetup
    {
        const BindingFlags OwnPublicMethods = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        readonly ConeMethodClassifier classifier;

        public ConeFixtureSetup(IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) {
            this.classifier = new ConeMethodClassifier(fixtureSink, testSink);
        }

        public void CollectFixtureMethods(Type type) {
            if(type == typeof(object))
                return;
            CollectFixtureMethods(type.BaseType);
            GetMethods(type).ForEach(Classify);
        }

        void Classify(MethodInfo method) { classifier.Classify(method); }

        MethodInfo[] GetMethods(Type type) {
            return type.GetMethods(OwnPublicMethods);
        }
    }
}
