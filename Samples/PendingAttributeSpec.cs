namespace Cone.Samples
{
    [Describe(typeof(PendingAttribute))]
    public class PendingAttributeSpec
    {
        [Pending]
        public void without_reason() { }

        [Pending(Reason = "for some reason")]
        public void for_some_reason() { }

        [Context("work in process context"), Pending]
        public class WipContext {
            public void children_will_be_pending() { }
        }
    }
}
