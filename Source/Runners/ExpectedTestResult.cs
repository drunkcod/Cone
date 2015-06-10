using System;
using System.Collections.Generic;

namespace Cone.Runners
{
	public enum ExpectedTestResultType 
	{
		None,
		Value,
		Exception
	}

	public struct ExpectedTestResult
	{
		public readonly ExpectedTestResultType ResultType;
		readonly object expectedResult;

		ExpectedTestResult(ExpectedTestResultType resultType, object value) {
			this.ResultType = resultType;
			this.expectedResult = value;
		}

		public static readonly ExpectedTestResult None = new ExpectedTestResult(ExpectedTestResultType.None, null);

		public static ExpectedTestResult Value(object value) {
			return new ExpectedTestResult(ExpectedTestResultType.Value, value);
		}

		public static ExpectedTestResult Exception(Type exceptionType, bool allowDerived) {
			return new ExpectedTestResult(ExpectedTestResultType.Exception, new KeyValuePair<Type, bool>(exceptionType, allowDerived));
		}

		public bool Matches(object obj) {
			switch(ResultType) {
				case ExpectedTestResultType.Exception:
					var x = (KeyValuePair<Type,bool>)expectedResult;
					if(x.Value == false) 
						return x.Key == obj.GetType();
					return x.Key.IsInstanceOfType(obj);
				default: return Convert.ChangeType(obj, expectedResult.GetType()).Equals(expectedResult);
			}
		}

		public override string ToString() {
			switch(ResultType) {
				case ExpectedTestResultType.None: return string.Empty;
				case ExpectedTestResultType.Exception: return ((KeyValuePair<Type,bool>)expectedResult).Key.FullName;
				default: return expectedResult.ToString();
			}
		}
	}
}