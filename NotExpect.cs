namespace Cone
{
    class NotExpect : IExpect
    {
        readonly IExpect inner;

        public NotExpect(IExpect inner) { this.inner = inner; }


        public object Actual {
            get { return inner.Actual; }
        }

        public bool Check() {
            return !inner.Check();
        }

        public string FormatBody(IExpressionFormatter formatter) {
            return string.Format("!({0})", inner.FormatBody(formatter));
        }

        public string FormatValues(IExpressionFormatter formatter) {
            return string.Empty;
        }
    }
}