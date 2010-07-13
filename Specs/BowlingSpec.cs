using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
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

    public class Bowling
    {
        public int Score { get { return 0; } }
        public void Hit(int pins) { }
    }

    static class Int32Extensions
    {
        public static void Times(this int count, Action action) {
            for (int i = 0; i != count; ++i)
                action();
        }
    }
}
