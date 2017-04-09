using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace procedural_map {
    static class Random {
        private static System.Random r = new System.Random(DateTime.Now.Millisecond);
        public static int Next() { return r.Next(); }
        public static int Next(int maxValue) { return r.Next(maxValue); }
        public static int Next(int minValue, int maxValue) { return r.Next(minValue, maxValue); }
    }
}
