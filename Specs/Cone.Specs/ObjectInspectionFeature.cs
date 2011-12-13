using Cone.Core;

namespace Cone
{
    [Feature("Object inspection")]
    public class ObjectInspectionFeature
    {
        public void displays_public_properties() {
            Verify.That(() => new { Int = 1, String = "s", Float = 3.14 }.Inspect() == "{ Float = 3.14, Int = 1, String = \"s\" }");
        }

        class MyClass 
        {
            public int Value;
        }

        public void displays_public_fields() {
            Verify.That(() => new MyClass { Value = 42 }.Inspect() == "{ Value = 42 }");
        }

        public void quote_strings() {
            Verify.That(() => "Hello".Inspect() == "\"Hello\"");
        }

        public void sequence_formatting() {
            Verify.That(() => new[]{1, 2, 3}.Inspect() == "new[] { 1, 2, 3 }");
        }
    }
}
