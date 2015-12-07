namespace Cone
{
    public class BoolDisplay 
    {
        string value;

        public BoolDisplay(bool value) : this(value, "true", "false") { }

        public BoolDisplay(bool value, string whenTrue, string whenFalse) {
            this.value = value ? whenTrue : whenFalse;
        }

        public override string ToString() {
            return value;
        }
    }
}
