using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace procedural_map {
    static class Mouse {
        private static double _x;
        public static double X {
            get { return _x; }
            set {
                _lastX = _x;
                _x = value;
            }
        }

        private static double _y;
        public static double Y {
            get { return _y; }
            set {
                _lastY = _y;
                _y = value;
            }
        }

        private static double _lastX;
        private static double _lastY;
        public static double DeltaX { get { return X - _lastX; } }
        public static double DeltaY { get { return Y - _lastY; } }

        public static string CoordinatesString { get { return "{" + X.ToString("F") + ", " + Y.ToString("F") + "}"; } }

        //public static double PressedX { get; set; }
        //public static double PressedY { get; set; }
        //public static string PressedString { get { return "{" + PressedX.ToString("F") + ", " + PressedY.ToString("F") + "}"; } }

        public static bool LeftButtonDown { get; set; }
        public static bool RightButtonDown { get; set; }

        static Mouse() {
            LeftButtonDown = false;
            RightButtonDown = false;
        }
    }
}
