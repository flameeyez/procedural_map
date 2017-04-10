using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace procedural_map {
    static class Debug {
        public static List<long> ChunkLoadTimes = new List<long>();
        public static List<string> Strings = new List<string>();
        public static string LastDrawTime { get; set; }
        public static string LastUpdateTime { get; set; }
        public static int TotalChunkCount { get { return Map.DebugChunkCount; } }
        public static int OnScreenChunkCount = 0;

        public static void Draw(CanvasAnimatedDrawEventArgs args) {
            int x = 1500;
            int y = 20;

            Strings.Clear();
            Strings.Add("Draw: " + LastDrawTime + "ms");
            Strings.Add("Update: " + LastUpdateTime + "ms");
            Strings.Add("Camera offset: " + Camera.ToString());
            Strings.Add("Chunk count: " + TotalChunkCount.ToString());
            Strings.Add("Chunks on screen: " + OnScreenChunkCount.ToString());
            Strings.Add("Average chunk load time: " + ChunkLoadTimes.Average().ToString("F") + "ms");
            Strings.Add("Last chunk load time: " + ChunkLoadTimes.Last().ToString() + "ms");

            foreach (string str in Strings) {
                args.DrawingSession.DrawText(str, new Vector2(x, y), Colors.White);
                y += 20;
            }
        }

        public static void Update(CanvasAnimatedUpdateEventArgs args) {
            OnScreenChunkCount = 0;
        }
    }
}
