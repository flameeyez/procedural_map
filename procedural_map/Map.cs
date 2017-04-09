using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;

namespace procedural_map {
    static class Map {
        public static int TILE_RESOLUTION = 32;
        private static Dictionary<Point, Chunk> ChunkCache = new Dictionary<Point, Chunk>();

        public static void Draw(CanvasAnimatedDrawEventArgs args) {
            foreach (Chunk chunk in ChunkCache.Values) { chunk.Draw(args); }
        }

        public static void Update(CanvasAnimatedUpdateEventArgs args) {
            // remove stale chunks
        }

        public static void Initialize(CanvasDevice device) {
            // load initial chunks; center on 0,0

            int initialChunks = 50;

            // load -1,-1 through 1,1
            for (int i = -initialChunks; i <= initialChunks; i++) {
                for (int j = -initialChunks; j <= initialChunks; j++) {
                    ChunkCache.Add(new Point(i, j), Chunk.Create(device, i, j));
                }
            }
        }
    }
}