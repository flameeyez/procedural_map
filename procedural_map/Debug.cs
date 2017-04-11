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
        public static object TimedStringsLock = new object();
        public static List<long> ChunkLoadTimes = new List<long>();
        public static List<string> Strings = new List<string>();
        public static long LastDrawTime { get; set; }
        public static long LastUpdateTime { get; set; }
        public static long LastDebugUpdateTime { get; set; }
        public static long LastFullLoopTime { get; set; }
        public static long MaxFullLoopTime { get; set; }
        public static int TotalChunkCount { get { return Map.DebugChunkCount; } }
        public static int OnScreenChunkCount = 0;
        public static int SlowFrames = 0;
        

        public static List<TimedString> TimedStrings = new List<TimedString>();

        public static void Draw(CanvasAnimatedDrawEventArgs args) {
            Strings.Clear();
            Strings.Add("Mouse: " + Mouse.CoordinatesString);
            Strings.Add("Mouse left: " + (Mouse.LeftButtonDown ? "DOWN" : "UP"));
            Strings.Add("Mouse right: " + (Mouse.RightButtonDown ? "DOWN" : "UP"));
            // Strings.Add("Mouse pressed: " + Mouse.PressedString);
            Strings.Add("Draw: " + LastDrawTime.ToString() + "ms");
            Strings.Add("Update: " + LastUpdateTime.ToString() + "ms");
            Strings.Add("Debug update: " + LastDebugUpdateTime.ToString() + "ms");
            Strings.Add("Full loop: " + LastFullLoopTime.ToString() + "ms");
            Strings.Add("Full loop (max): " + MaxFullLoopTime.ToString() + "ms");
            Strings.Add("Slow frames: " + SlowFrames.ToString());
            Strings.Add("Camera offset: " + Camera.CoordinatesString());
            Strings.Add("Camera offset (chunk): " + Camera.ChunkPositionString());
            Strings.Add("Camera offset (tile): " + Camera.ChunkTilePositionString());
            if(Map.DebugChunkCount > 0) {
                Strings.Add("Chunk count: " + TotalChunkCount.ToString());
                Strings.Add("Chunks on screen: " + OnScreenChunkCount.ToString());
                Strings.Add("Average chunk load time: " + ChunkLoadTimes.Average().ToString("F") + "ms");
                Strings.Add("Last chunk load time: " + ChunkLoadTimes.Last().ToString() + "ms");
            }

            int x = 1500;
            int y = 20;
            int width = 410;
            int height = (Strings.Count + 1) * 20;
            Color backgroundColor = Colors.CornflowerBlue;
            Color borderColor = Colors.White;
            args.DrawingSession.FillRectangle(new Windows.Foundation.Rect(x - 5, y - 5, width, height), backgroundColor);
            args.DrawingSession.DrawRoundedRectangle(new Windows.Foundation.Rect(x - 5, y - 5, width, height), 3, 3, borderColor);
            foreach (string str in Strings) {
                args.DrawingSession.DrawText(str, new Vector2(x, y), Colors.White);
                y += 20;
            }

            if (TimedStrings.Count > 0) {
                y += 50;
                height = (TimedStrings.Count + 1) * 20;
                args.DrawingSession.FillRectangle(new Windows.Foundation.Rect(x - 5, y - 5, width, height), backgroundColor);
                args.DrawingSession.DrawRoundedRectangle(new Windows.Foundation.Rect(x - 5, y - 5, width, height), 3, 3, borderColor);
                lock (Debug.TimedStringsLock) {
                    foreach (TimedString str in TimedStrings) {
                        str.Draw(args, new Vector2(x, y));
                        y += 20;
                    }
                }
            }
        }

        public static void Update(CanvasAnimatedUpdateEventArgs args) {
            OnScreenChunkCount = 0;
            for (int i = TimedStrings.Count - 1; i >= 0; i--) {
                if (TimedStrings[i].Dead) { TimedStrings.RemoveAt(i); }
                else { TimedStrings[i].Update(args); }
            }
        }

        public static void AddTimedString(string str) {
            lock (Debug.TimedStringsLock) {
                TimedStrings.Add(new TimedString(str));
            }
        }
    }
}
