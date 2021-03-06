using System;
using CheckThat;
using Cone.Core;

namespace Cone
{
	[Describe(typeof(RowBuilder<>))]
    public class RowBuilderSpec
    {
        void DoStuff(object arg0) { }
        void Lambda(Func<int> f){}
        int Double(int x){ return x + x; }

		RowBuilder<RowBuilderSpec> NewRowBuilder() =>
			new RowBuilder<RowBuilderSpec>(new ConeTestNamer());

        public void collects_constants_parameters() {
            var rows = NewRowBuilder()
                .Add(x => x.DoStuff(42));
            
            Check.That(() => rows[0].Parameters[0] == (object)42);
        }

        public void collects_computed_parameters() {
            var rows = NewRowBuilder()
                .Add(x => x.DoStuff(Double(21)));
            
            Check.That(() => rows[0].Parameters[0] == (object)42);
        }

        public void collect_lambda() {
            Func<int> lambda = () => 42;
            var rows = NewRowBuilder()
                .Add(x => x.Lambda(lambda));
            Check.That(() => (Func<int>)rows[0].Parameters[0] == lambda);
        }

        public void collect_inline_lambda() {
            var rows = NewRowBuilder()
                .Add(x => x.Lambda(() => 42));
            Check.That(() => rows[0].Parameters[0] is Func<int>);
        }
}
}
