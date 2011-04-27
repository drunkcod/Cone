namespace Cone.Samples
{
    [Describe(typeof(PendingAttribute))]
    public class PendingAttributeSamples
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

    [Describe(typeof(PendingSpecSample)), Pending]
    public class PendingSpecSample {
        public void this_is_pending() { }
    }

    [Feature("pending feature"), Pending]
    public class PendingFeatureSample {
        public void this_is_pending() { }
    }
}
