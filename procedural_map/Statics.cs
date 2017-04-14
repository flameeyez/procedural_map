using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;

namespace procedural_map {
    static class Statics {
        public static double ClientWidth;
        public static double ClientHeight;

        internal static Color RandomColor() {
            byte red = (byte)Random.Next(255);
            byte green = (byte)Random.Next(255);
            byte blue = (byte)Random.Next(255);
            return Color.FromArgb(255, red, green, blue);
        }

        internal static void Initialize(CanvasAnimatedControl canvasMain) {
            ClientWidth = canvasMain.ActualWidth;
            ClientHeight = canvasMain.ActualHeight;
        }
    }
}