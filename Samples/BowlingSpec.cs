using System;
using NUnit.Framework;

namespace Cone.Samples.Specs
{
    [Describe(typeof(Bowling), "Score")]
    public class BowlingSpec
    {
        public void returns_0_for_gutter_game() {
            var bowling = new Bowling();
            20.Times(() => bowling.Hit(0));
            Verify.That(() => bowling.Score == 0);
        }
    }
}

namespace Cone.Samples.Tests
{
    public class BowlingTests
    {
        [Test]
        public void returns_0_for_gutter_game() {
            var bowling = new Bowling();
            20.Times(() => bowling.Hit(0));
            Assert.That(bowling.Score, Is.EqualTo(0));
        }
    }
}
