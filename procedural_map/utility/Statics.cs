using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Microsoft.Graphics.Canvas;

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

        internal static async void Initialize(CanvasAnimatedControl canvasMain) {
            ClientWidth = canvasMain.ActualWidth;
            ClientHeight = canvasMain.ActualHeight;
            BitmapOverworld = await CanvasBitmap.LoadAsync(canvasMain.Device, "images/overworld.png");
        }

        public static CanvasBitmap BitmapOverworld { get; set; }
        public static PointInt OverworldTileMountain = new PointInt(0, 0);
        public static PointInt OverworldTileGrass = new PointInt(32, 0);
        public static PointInt OverworldTileForest = new PointInt(64, 0);
        public static PointInt OverworldTileDesert = new PointInt(96, 0);
        public static PointInt OverworldTileWater = new PointInt(128, 0);
        public static PointInt OverworldTileGrassLight = new PointInt(160, 0);
        public static int OverworldTileResolution = 32;
    }
}