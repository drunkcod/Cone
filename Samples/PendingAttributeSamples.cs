namespace Cone.Samples
{
    [Describe(typeof(PendingAttribute))]
    public class PendingAttributeSamples
    {
        [Pending]
        public void without_reason() { Check.That(() => false);}

        [Pending(Reason = "for some reason")]
        public void for_some_reason() { Check.That(() => false);}

        [Context("work in process context"), Pending]
        public class WipContext {
            public void children_will_be_pending() { Check.That(() => false);}
        }
    }

    [Describe(typeof(PendingSpecSample)), Pending]
    public class PendingSpecSample {
        public void this_is_pending() { Check.That(() => false); }
    }

    [Feature("pending feature"), Pending]
    public class PendingFeatureSample {
        public void this_is_pending() { Check.That(() => false); }
    }
}
