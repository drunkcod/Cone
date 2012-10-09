using System;

namespace Cone
{
    [Describe(typeof(RowBuilder<>))]
    public class RowBuilderSpec
    {
        void DoStuff(object arg0) { }
        void Lambda(Func<int> f){}
        int Double(int x){ return x + x; }

        public void collects_constants_parameters() {
            var rows = new RowBuilder<RowBuilderSpec>()
                .Add(x => x.DoStuff(42));
            
            Verify.That(() => rows[0].Parameters[0] == (object)42);
        }

        public void collects_computed_parameters() {
            var rows = new RowBuilder<RowBuilderSpec>()
                .Add(x => x.DoStuff(Double(21)));
            
            Verify.That(() => rows[0].Parameters[0] == (object)42);
        }

        public void collect_lambda() {
            Func<int> lambda = () => 42;
            var rows = new RowBuilder<RowBuilderSpec>()
                .Add(x => x.Lambda(lambda));
            Verify.That(() => (Func<int>)rows[0].Parameters[0] == lambda);
        }

        public void collect_inline_lambda() {
            var rows = new RowBuilder<RowBuilderSpec>()
                .Add(x => x.Lambda(() => 42));
            Verify.That(() => rows[0].Parameters[0] is Func<int>);
        }
}
}
