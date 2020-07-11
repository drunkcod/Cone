namespace CheckThat.Expectations
{
	class ExpectedNull
    {
		ExpectedNull() { }
        public static readonly ExpectedNull Value = new ExpectedNull();
        
        public override bool Equals(object obj) {
            return obj == null;
        }

        public override int GetHashCode() => 0;

        public override string ToString() => "null";
    }
}
