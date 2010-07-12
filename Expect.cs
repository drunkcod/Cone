namespace Cone
{
    class ExpectNull
    {
        public override bool Equals(object obj) {
            return obj == null;
        }

        public override int GetHashCode() {
            return 0;
        }
    }

    class Expect
    {
        const string EqualFormat = "  {0} wasn't equal to {1}.\n  Expected {3}\nActual {2}";
        const string NotEqualFormat = "  {0} was equal to {1}.\n  Didn't expect {3}";
        static readonly ExpectNull ExpectNull = new ExpectNull();

        readonly object actual;
        readonly object expected;

        public static Expect New<TActual, TExpected>(TActual actual, TExpected expected) { return new Expect(actual, expected); }

        Expect(object actual, object expected) {
            this.actual = actual ?? ExpectNull;
            this.expected = expected;
        }

        public bool Equal() { 
            return actual.Equals(expected); 
        }

        public string FormatEqual(string actualDisplay, string expectedDisplay) {
            return Format(EqualFormat, actualDisplay, expectedDisplay);
        }

        public string FormatNotEqual(string actualDisplay, string expectedDisplay) {
            return Format(NotEqualFormat, actualDisplay, expectedDisplay);
        }

        string Format(string format, string actualDisplay, string expectedDisplay) {
            return string.Format(format, actualDisplay, expectedDisplay, actual, expected);
        }
    }
}
