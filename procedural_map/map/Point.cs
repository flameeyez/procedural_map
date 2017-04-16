using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace procedural_map {
    struct Point {
        public int X { get; set; }
        public int Y { get; set; }
        public Point(int x, int y) { X = x; Y = y; }
        public override bool Equals(object obj) {
            Point compare = (Point)obj;
            return (compare.X == X && compare.Y == Y);
        }
        public override int GetHashCode() {
            return X.GetHashCode() * 17 + Y.GetHashCode();
        }
    }
}
