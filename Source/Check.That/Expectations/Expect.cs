using System.Linq.Expressions;

namespace CheckThat.Expectations
{
	public class Expect : BooleanExpect 
    {
        readonly IExpectValue expected;

        public Expect(Expression body, IExpectValue actual, IExpectValue expected) : base(body, actual) {            
            this.expected = expected.Value != null ? expected : ExpectValue.Null;
        }

        protected override IExpectValue Expected => expected;
    }
}
