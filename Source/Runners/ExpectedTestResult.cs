using System;

namespace Cone.Runners
{
	[Flags]
	public enum ExpectedTestResultType : byte
	{
		None = 0,
		Value = 1,
		Exception = 1 << 1,
		TypeMask = None | Value | Exception,
		ExceptionAllowDerived = 1 << 2,
	}

	public struct ExpectedTestResult
	{
		readonly object expectedResult;
		readonly ExpectedTestResultType resultType;

		public ExpectedTestResultType ResultType { 
			get { 
				return resultType & ExpectedTestResultType.TypeMask; 
			}
		}

		ExpectedTestResult(ExpectedTestResultType resultType, object value) {
			this.resultType = resultType;
			this.expectedResult = value;
		}

		public static readonly ExpectedTestResult None = new ExpectedTestResult(ExpectedTestResultType.None, null);

		public static ExpectedTestResult Value(object value) {
			return new ExpectedTestResult(ExpectedTestResultType.Value, value);
		}

		public static ExpectedTestResult Exception(Type exceptionType, bool allowDerived) {
			return new ExpectedTestResult(ExpectedTestResultType.Exception | (allowDerived ? ExpectedTestResultType.ExceptionAllowDerived : 0), exceptionType);
		}

		public bool Matches(object obj) {
			switch(ResultType) {
				case ExpectedTestResultType.Exception:
					var x = (Type)expectedResult;
					if((resultType & ExpectedTestResultType.ExceptionAllowDerived) == 0) 
						return x == obj.GetType();
					return x.IsInstanceOfType(obj);
				default: return Convert.ChangeType(obj, expectedResult.GetType()).Equals(expectedResult);
			}
		}

		public override string ToString() {
			switch(ResultType) {
				case ExpectedTestResultType.None: return string.Empty;
				case ExpectedTestResultType.Exception: return ((Type)expectedResult).FullName;
				default: return expectedResult.ToString();
			}
		}
	}
}