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

        private static Dictionary<Point, Chunk> ChunkCache = new Dictionary<Point, Chunk>();
        public static int DebugChunkCount { get { return ChunkCache.Count; } }

        private static double dMillisecondsSinceLastCacheClear = 0.0;
        private static int nCacheClearThresholdMilliseconds = 5000;
        private static double dMillisecondsSinceLastCache = 0.0;
        private static int nCacheThresholdMilliseconds = 2000;

        private static bool bPauseCaching = true;

        public static void Draw(CanvasAnimatedDrawEventArgs args) {
            lock(Chunk.CacheLock) {
                for(int x = Camera.ChunkPositionX; x < Camera.ChunkPositionX + Chunk.MaxChunksVisibleX; x++) {
                    for(int y = Camera.ChunkPositionY; y < Camera.ChunkPositionY + Chunk.MaxChunksVisibleY; y++) {
                        Chunk c;
                        if(ChunkCache.TryGetValue(new Point(x, y), out c)) {
                            c.Draw(args);
                        }
                        //ChunkCache[new Point(x, y)].Draw(args);
                    }
                }
                // foreach (Chunk chunk in Chunks) { chunk.Draw(args); }
            }
        }

        public async static Task Update(CanvasAnimatedUpdateEventArgs args) {
            dMillisecondsSinceLastCacheClear += args.Timing.ElapsedTime.TotalMilliseconds;
            if(dMillisecondsSinceLastCacheClear > nCacheClearThresholdMilliseconds) {
                dMillisecondsSinceLastCacheClear = 0.0;
                await Task.Run(() => CacheCleanup());
            }

            dMillisecondsSinceLastCache += args.Timing.ElapsedTime.TotalMilliseconds;
            if(dMillisecondsSinceLastCache > nCacheThresholdMilliseconds) {
                dMillisecondsSinceLastCache = 0.0;
                await Task.Run(() => Cache());
            }
        }

        public async static void Initialize(CanvasDevice device) {
            _device = device;
            long before = GC.GetTotalMemory(true);
            Chunk c = await Task.Run(() => Chunk.Create(_device, 0, 0));
            long after = GC.GetTotalMemory(true);
            GC.KeepAlive(c);
            Debug.ChunkSizeMB = "Chunk size (MB): " + ((after - before) / 1000000.0f).ToString("F") + "MB";
            lock(Chunk.CacheLock) {
                ChunkCache.Add(c.Coordinates, c);
            }

            Debug.AddTimedString("Creating initial chunks...", Colors.White);
            // load {-initialChunks, -initialChunks} through {initialChunks, initialChunks}
            for(int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                for(int j = -_cachedChunkLoadRadius; j <= _cachedChunkLoadRadius; j++) {
                    if(i == 0 && j == 0) { continue; }
                    await Task.Run(() => CacheChunk(i, j, bSuppressOutput: true));
                }
            }
            Debug.AddTimedString("Initial chunks created!", Colors.Green);
            bPauseCaching = false;
        }

        public static bool CacheChunk(int x, int y, bool bSuppressOutput = false) {
            Chunk c;
            Point point = new Point(x, y);

            if(!ChunkCache.TryGetValue(point, out c)) {
                // if (!bSuppressOutput) { Debug.AddTimedString("Caching chunk: {" + x.ToString() + ", " + y.ToString() + "}"); }
                c = Chunk.Create(_device, x, y);

                Chunk temp;
                lock(Chunk.CacheLock) {
                    if(!ChunkCache.TryGetValue(point, out temp)) {
                        ChunkCache.Add(c.Coordinates, c);
                    }
                }

                return true;
            }
            else {
                return false;
            }
        }

        public static async Task Cache() {
            if(bPauseCaching) { return; }

            int nChunksAdded = 0;
            Stopwatch s = Stopwatch.StartNew();

            Debug.AddTimedString("Updating cache...", Colors.Yellow);
            Point p = new Point(Camera.ChunkPositionX, Camera.ChunkPositionY);
            if(await Task.Run(() => CacheChunk(p.X, p.Y))) { nChunksAdded++; }

            for(int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                for(int j = -_cachedChunkLoadRadius; j <= _cachedChunkLoadRadius; j++) {
                    if(i == 0 && j == 0) { continue; }
                    if(await Task.Run(() => CacheChunk(p.X + i, p.Y + j, bSuppressOutput: true))) { nChunksAdded++; }
                }
            }

            s.Stop();
            Debug.AddTimedString("Cache update took " + s.ElapsedMilliseconds.ToString() + "ms", Colors.White);
            Debug.AddTimedString("Chunks added: " + nChunksAdded.ToString(), Colors.Green);
        }

        public static void CacheCleanup() {
            if(bPauseCaching) { return; }

            Stopwatch s = Stopwatch.StartNew();
            Debug.AddTimedString("Cleaning up cache...", Colors.Yellow);
            Dictionary<Point, Chunk> swap = new Dictionary<Point, Chunk>();
            foreach(KeyValuePair<Point, Chunk> chunk in ChunkCache) {
                if((Math.Abs(Camera.ChunkPositionX - chunk.Value.Coordinates.X) < _cachedChunkUnloadThreshold)
                && (Math.Abs(Camera.ChunkPositionY - chunk.Value.Coordinates.Y) < _cachedChunkUnloadThreshold)) {
                    swap.Add(chunk.Key, chunk.Value);
                }
            }
            int nChunksRemoved = ChunkCache.Count - swap.Count;
            lock(Chunk.CacheLock) {
                ChunkCache = swap;
            }
            s.Stop();
            Debug.AddTimedString("Cache cleanup took " + s.ElapsedMilliseconds.ToString() + "ms", Colors.White);
            Debug.AddTimedString("Chunks removed: " + nChunksRemoved.ToString(), Colors.Red);
        }

        public async static void CacheUp(int x, int y) {
            for(int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                await Task.Run(() => CacheChunk(x + i, y - _cachedChunkLoadRadius));
            }
        }

        public async static void CacheDown(int x, int y) {
            for(int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                await Task.Run(() => CacheChunk(x + i, y + _cachedChunkLoadRadius));
            }
        }

        public async static void CacheLeft(int x, int y) {
            for(int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                await Task.Run(() => CacheChunk(x - _cachedChunkLoadRadius, y + i));
            }
        }

        public async static void CacheRight(int x, int y) {
            for(int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                await Task.Run(() => CacheChunk(x + _cachedChunkLoadRadius, y + i));
            }
        }
    }
}

// old cache cleanup
//for(int i = Chunks.Count - 1; i >= 0; i--) {
//    if((Math.Abs(Camera.ChunkPositionX - Chunks[i].ChunkCoordinateX) >= _cachedChunkUnloadThreshold)
//    || (Math.Abs(Camera.ChunkPositionY - Chunks[i].ChunkCoordinateY) >= _cachedChunkUnloadThreshold)) {
//        //Debug.AddTimedString("Removing chunk: {" + Chunks[i].ChunkCoordinateX.ToString() + ", " + Chunks[i].ChunkCoordinateY.ToString() + "}");
//        nChunksRemoved++;
//        ChunkCacheMap.Remove(new Point(Chunks[i].ChunkCoordinateX, Chunks[i].ChunkCoordinateY));
//        Chunks.RemoveAt(i);
//    }
//}