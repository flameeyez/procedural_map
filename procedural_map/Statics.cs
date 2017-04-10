using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace procedural_map {
    static class Statics {
        public static int ClientWidth = 1920;
        public static int ClientHeight = 1080;

        public static Color RandomColor() {
            byte red = (byte)Random.Next(255);
            byte green = (byte)Random.Next(255);
            byte blue = (byte)Random.Next(255);
            return Color.FromArgb(255, red, green, blue);
        }
    }
}
