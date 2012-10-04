namespace Cone.Expectations
{
    public static class ExpectMessages
    {        
        public const string EqualFormat = "  Expected: {1}\n  But was:  {0}";
        public const string NotEqualFormat = "  Didn't expect both to be {1}";
        public const string LessThanFormat = " Expected: less than {1}\n  But was:  {0}";
        public const string LessThanOrEqualFormat = " Expected: less than or equal to {1}\n  But was:  {0}";
        public const string GreaterThanFormat = " Expected: greater than {1}\n  But was:  {0}";
        public const string GreaterThanOrEqualFormat = " Expected: greater than or equal to {1}\n  But was:  {0}";
        public const string MissingExceptionFormat = "{0} didn't raise an exception.";
        public const string UnexpectedExceptionFormat = "{0} raised the wrong type of Exception\nExpected: {1}\nActual: {2}";
    }
}
