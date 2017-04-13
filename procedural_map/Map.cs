using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI;

namespace procedural_map {
    static class Map {
        private static CanvasDevice _device;
        private static int _cachedChunkLoadRadius = 3;
        private static int _cachedChunkUnloadThreshold = 5;
        public static int TILE_RESOLUTION = 32;

        private static HashSet<Point> ChunkCacheMap = new HashSet<Point>();
        private static List<Chunk> Chunks = new List<Chunk>();
        public static int DebugChunkCount { get { return Chunks.Count; } }

        private static double dMillisecondsSinceLastCacheClear = 0.0;
        private static int nCacheClearThresholdMilliseconds = 5000;
        private static double dMillisecondsSinceLastCache = 0.0;
        private static int nCacheThresholdMilliseconds = 2000;

        private static bool bPauseCaching = true;

        public static void Draw(CanvasAnimatedDrawEventArgs args) {
            lock (Chunk.CacheLock) {
                foreach (Chunk chunk in Chunks) { chunk.Draw(args); }
            }
        }

        public async static Task Update(CanvasAnimatedUpdateEventArgs args) {
            dMillisecondsSinceLastCacheClear += args.Timing.ElapsedTime.TotalMilliseconds;
            if (dMillisecondsSinceLastCacheClear > nCacheClearThresholdMilliseconds) {
                dMillisecondsSinceLastCacheClear = 0.0;
                await Task.Run(() => CacheCleanup());
            }

            dMillisecondsSinceLastCache += args.Timing.ElapsedTime.TotalMilliseconds;
            if (dMillisecondsSinceLastCache > nCacheThresholdMilliseconds) {
                dMillisecondsSinceLastCache = 0.0;
                await Task.Run(() => Cache());
            }
        }

        public async static void Initialize(CanvasDevice device) {
            _device = device;
            Chunk c = await Task.Run(() => Chunk.Create(_device, 0, 0));
            lock (Chunk.CacheLock) {
                ChunkCacheMap.Add(new Point(0, 0));
                Chunks.Add(c);
            }

            Debug.AddTimedString("Creating initial chunks...", Colors.White);
            // load {-initialChunks, -initialChunks} through {initialChunks, initialChunks}
            for (int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                for (int j = -_cachedChunkLoadRadius; j <= _cachedChunkLoadRadius; j++) {
                    if (i == 0 && j == 0) { continue; }
                    await CacheChunk(i, j, bSuppressOutput: true);
                }
            }
            Debug.AddTimedString("Initial chunks created!", Colors.Green);
            bPauseCaching = false;
        }

        public async static Task<bool> CacheChunk(int x, int y, bool bSuppressOutput = false) {
            Chunk c;
            Point point = new Point(x, y);

            if (!ChunkCacheMap.Contains(point)) {
                // if (!bSuppressOutput) { Debug.AddTimedString("Caching chunk: {" + x.ToString() + ", " + y.ToString() + "}"); }
                c = await Task.Run(() => Chunk.Create(_device, x, y));

                lock (Chunk.CacheLock) {
                    if (ChunkCacheMap.Add(point)) {
                        Chunks.Add(c);
                    }
                }

                return true;
            }
            else {
                return false;
            }            
        }

        public static async Task Cache() {
            if (bPauseCaching) { return; }

            int nChunksAdded = 0;
            Stopwatch s = Stopwatch.StartNew();

            Debug.AddTimedString("Updating cache...", Colors.Yellow);
            Point p = new Point(Camera.ChunkPositionX, Camera.ChunkPositionY);
            if(await CacheChunk(p.X, p.Y)) { nChunksAdded++; }

            for (int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                for (int j = -_cachedChunkLoadRadius; j <= _cachedChunkLoadRadius; j++) {
                    if (i == 0 && j == 0) { continue; }
                    if(await CacheChunk(p.X + i, p.Y + j, bSuppressOutput: true)) { nChunksAdded++; }
                }
            }

            s.Stop();
            Debug.AddTimedString("Cache update took " + s.ElapsedMilliseconds.ToString() + "ms", Colors.White);
            Debug.AddTimedString("Chunks added: " + nChunksAdded.ToString(), Colors.Green);
        }

        public static void CacheCleanup() {
            if (bPauseCaching) { return; }

            int nChunksRemoved = 0;
            Stopwatch s = Stopwatch.StartNew();

            Debug.AddTimedString("Cleaning up cache...", Colors.Yellow);
            lock (Chunk.CacheLock) {
                for (int i = Chunks.Count - 1; i >= 0; i--) {
                    if ((Math.Abs(Camera.ChunkPositionX - Chunks[i].ChunkCoordinateX) >= _cachedChunkUnloadThreshold)
                    || (Math.Abs(Camera.ChunkPositionY - Chunks[i].ChunkCoordinateY) >= _cachedChunkUnloadThreshold)) {
                        //Debug.AddTimedString("Removing chunk: {" + Chunks[i].ChunkCoordinateX.ToString() + ", " + Chunks[i].ChunkCoordinateY.ToString() + "}");
                        nChunksRemoved++;
                        ChunkCacheMap.Remove(new Point(Chunks[i].ChunkCoordinateX, Chunks[i].ChunkCoordinateY));
                        Chunks.RemoveAt(i);
                    }
                }
            }

            s.Stop();
            Debug.AddTimedString("Cache cleanup took " + s.ElapsedMilliseconds.ToString() + "ms", Colors.White);
            Debug.AddTimedString("Chunks removed: " + nChunksRemoved.ToString(), Colors.Red);
        }

        public async static void CacheUp(int x, int y) {
            for (int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                await CacheChunk(x + i, y - _cachedChunkLoadRadius);
                await Task.Yield();
            }
        }

        public async static void CacheDown(int x, int y) {
            for (int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                await CacheChunk(x + i, y + _cachedChunkLoadRadius);
                await Task.Yield();
            }
        }

        public async static void CacheLeft(int x, int y) {
            for (int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                await CacheChunk(x - _cachedChunkLoadRadius, y + i);
                await Task.Yield();
            }
        }

        public async static void CacheRight(int x, int y) {
            for (int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                await CacheChunk(x + _cachedChunkLoadRadius, y + i);
                await Task.Yield();
            }
        }
    }
}