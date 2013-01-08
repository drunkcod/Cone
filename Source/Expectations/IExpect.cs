using System;
using System.Linq.Expressions;
using Cone.Core;

namespace Cone.Expectations
{
    public struct CheckResult
    {
        public readonly bool IsSuccess;
        public readonly Maybe<object> Actual;
		public readonly Maybe<object> Expected;

		public CheckResult(bool success, Maybe<object> actual, Maybe<object> expected) {
			this.IsSuccess = success;
			this.Actual = actual;
			this.Expected = expected;
		}

        public static bool operator==(CheckResult left, CheckResult right) {
            return left.IsSuccess == right.IsSuccess 
				&& Equals(left.Actual, right.Actual)
				&& Equals(left.Expected, right.Expected);
        }

        public static bool operator!=(CheckResult left, CheckResult right) {
            return !(left == right);
        }

        public override string ToString() {
            return string.Format("{{Success: {0}, Actual: {1}}}", IsSuccess, Actual);
        }

        public override bool Equals(object obj) {
            if(object.ReferenceEquals(obj, this))
                return true;
            return obj is CheckResult && (CheckResult)obj == this;
        }

        public override int GetHashCode() {
            return Actual.GetHashCode();
        }
    }

    public interface IExpect
    {
        CheckResult Check();
        string FormatExpression(IFormatter<Expression> formatter);
        string FormatActual(IFormatter<object> formatter);
        string FormatExpected(IFormatter<object> formatter);
        string FormatMessage(IFormatter<object> formatter);
    }
}