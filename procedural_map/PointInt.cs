using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace procedural_map {
    class PointInt {
        public int X { get; set; }
        public int Y { get; set; }
        public PointInt(int x, int y) { X = x; Y = y; }
        public override bool Equals(object obj) {
            PointInt compare = (PointInt)obj;
            return (compare.X == X && compare.Y == Y);
        }
        public override int GetHashCode() {
            return X.GetHashCode() * 17 + Y.GetHashCode();
        }
        public override string ToString() {
            return X.ToString() + ", " + Y.ToString();
        }
    }
}
