using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Cone.Samples
{
    [Describe(typeof(TutorialSpec))]
    public class TutorialSpec
    {
        [Explicit]
        public void this_test_should_be_explicit() {          
            Verify.That(() => false); 
        }

        [Context("null subexpression detection")]
        public class NullSubexpressionDetection
        {
            public void member_access_would_raise_NullReferenceException() {
                var foo = new { ThisValueIsNull = (string)null };
                Verify.That(() => foo.ThisValueIsNull.Length != 0); 
            }
            public void method_call_would_raise_NullReferenceException() {
                var foo = new { ThisValueIsNull = (string)null };
                Verify.That(() => foo.ThisValueIsNull.Contains("hello")); 
            }
        }

        [Context("DisplayAs")]
        public class DisplayAs
        {
            [DisplayAs("{0} + {1} == {2}")
            ,Row(1, 2, 3)]
            public void Add(int a, int b, int result) {
                Verify.That(() => a + b == result);
            }

            [DisplayAs("{0} - {1} == {2}", Heading = "When subtracting {1} from {0} the we get {2}")
            ,Row(3, 2, 1)]
            public void Subtract(int a, int b, int result) {
                Verify.That(() => a - b == result);
            }
        }

        public void foo() {
            Func<int> throws = () => { throw new InvalidOperationException(); }; 

            Verify.Throws<ArgumentException>.When(() => throws() == 42); 
        }

        public void report_failing_subexpression()
        {
            Func<int> throws = () => { throw new InvalidOperationException(); }; 
            Verify.That(() => throws() == 42); 
        }

        int Throws() { throw new NotImplementedException(); }
        
        public void wrong_subexpression_raises_exception() {
            Func<TutorialSpec> getTarget = () => { throw new InvalidOperationException(); };
            Verify.Throws<NotImplementedException>.When(() => getTarget().Throws());
        }

        public void report_failing_subexpression_call()
        {
            Verify.That(() => Throws() == 42); 
        }

        public void funky(Func<int, int> fun, int input, int result) {
            if(fun != null)
                Verify.That(() => fun(input) == result);
        }

        public IEnumerable<IRowTestData> FunkyRows() {
            return new RowBuilder<TutorialSpec>()
                .Add(x => x.funky(input => input + 1, 1, 2))
                .Add(x => x.funky(null, 0, 0));
        }

    }
}
