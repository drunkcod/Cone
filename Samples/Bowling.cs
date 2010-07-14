using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Samples
{
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
