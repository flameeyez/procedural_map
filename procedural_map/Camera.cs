using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.System;

namespace procedural_map {
    static class Camera {
        public static int PositionX { get; set; }
        public static int PositionY { get; set; }
        private static int _velocity = 15;

        internal static void KeyDown(VirtualKey vk) {
            switch (vk) {
                case VirtualKey.Up:
                    PositionY -= _velocity;
                    break;
                case VirtualKey.Down:
                    PositionY += _velocity;
                    break;
                case VirtualKey.Left:
                    PositionX -= _velocity;
                    break;
                case VirtualKey.Right:
                    PositionX += _velocity;
                    break;
            }
        }

        public static string ToString() {
            StringBuilder sb = new StringBuilder("{");
            sb.Append(PositionX.ToString());
            sb.Append(", ");
            sb.Append(PositionY.ToString());
            sb.Append("}");
            return sb.ToString();
        }
    }
}
