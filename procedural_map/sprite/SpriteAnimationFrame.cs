using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace procedural_map {
    class SpriteAnimationFrame {
        public int X { get; set; }
        public int Y { get; set; }

        private SpriteAnimationFrame() { }
        public SpriteAnimationFrame(int x, int y) {
            X = x;
            Y = y;
        }

        internal static SpriteAnimationFrame Copy(SpriteAnimationFrame frame) {
            SpriteAnimationFrame copy = new SpriteAnimationFrame();
            copy.X = frame.X;
            copy.Y = frame.Y;
            return copy;
        }
    }
}
