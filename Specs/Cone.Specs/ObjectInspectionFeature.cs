using Cone.Core;

namespace Cone
{
    [Feature("Object inspection")]
    public class ObjectInspectionFeature
    {
        public void displays_public_properties() {
            Check.That(() => new { Int = 1, String = "s", Float = 3.14 }.Inspect() == "{ Float = 3.14, Int = 1, String = \"s\" }");
        }

        public void displays_public_fields() {
            Check.That(() => new MyValue<int> { Value = 42 }.Inspect() == "{ Value = 42 }");
        }

        public void quote_strings() {
            Check.That(() => "Hello".Inspect() == "\"Hello\"");
        }

        public void sequence_formatting() {
            Check.That(() => new []{1, 2, 3}.Inspect() == "new [] { 1, 2, 3 }");
        }
    }
}
