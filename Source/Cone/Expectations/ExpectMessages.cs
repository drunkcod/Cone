using Cone.Core;

namespace Cone.Expectations
{
    public static class ExpectMessages
    {        
		public static ConeMessage EqualFormat(string actual, string expected) => EqualFormat(ConeMessage.Parse(actual), ConeMessage.Parse(expected));
        public static ConeMessage EqualFormat(ConeMessage actual, ConeMessage expected) => ConeMessage.Combine(
			ConeMessage.Parse("  Expected: "),
			expected, 
			ConeMessage.NewLine,
			ConeMessage.Parse("  But was:  "),
			actual);

        public const string NotEqualFormat = "  Didn't expect both to be {1}";
        public const string LessThanFormat = " Expected: less than {1}\n  But was:  {0}";
        public const string LessThanOrEqualFormat = " Expected: less than or equal to {1}\n  But was:  {0}";
        public const string GreaterThanFormat = " Expected: greater than {1}\n  But was:  {0}";
        public const string GreaterThanOrEqualFormat = " Expected: greater than or equal to {1}\n  But was:  {0}";
        public const string MissingExceptionFormat = "{0} didn't raise an exception.";
        public const string UnexpectedExceptionFormat = "{0} raised the wrong type of Exception\nExpected: {1}\nActual: {2}";
    }
}
