using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using System.Threading.Tasks;

namespace procedural_map {
    static class Map {
        public static int TILE_RESOLUTION = 32;
        private static Dictionary<Point, Chunk> ChunkCache = new Dictionary<Point, Chunk>();
        public static int DebugChunkCount { get { return ChunkCache.Count; } }

        public static void Draw(CanvasAnimatedDrawEventArgs args) {
            lock (Chunk.CacheLock) {
                foreach (Chunk chunk in ChunkCache.Values) { chunk.Draw(args); }
            }
        }

        public static void Update(CanvasAnimatedUpdateEventArgs args) {
            // remove stale chunks
        }

        public async static void Initialize(CanvasDevice device) {
            Chunk c = await Task.Run(() => Chunk.Create(device, 0, 0));
            lock (Chunk.CacheLock) {
                ChunkCache.Add(new Point(0, 0), c);
            }

            int initialChunks = 5;
            // load {-initialChunks, -initialChunks} through {initialChunks, initialChunks}
            for (int i = -initialChunks; i <= initialChunks; i++) {
                for (int j = -initialChunks; j <= initialChunks; j++) {
                    if (i == 0 && j == 0) { continue; }
                    Debug.AddTimedString("Creating chunk: {" + i.ToString() + ", " + j.ToString() + "}");
                    c = await Task.Run(() => Chunk.Create(device, i, j));
                    lock (Chunk.CacheLock) {
                        ChunkCache.Add(new Point(i, j), c);
                    }
                }
            }
        }
    }
}