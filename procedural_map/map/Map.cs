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
        private static int _cachedChunkLoadRadius = 5;//3;
        private static int _cachedChunkUnloadThreshold = 10;//5;
        public static int TILE_RESOLUTION = 4;//32;

        private static Dictionary<PointInt, Chunk> ChunkCache = new Dictionary<PointInt, Chunk>();
        public static int DebugChunkCount { get { return ChunkCache.Count; } }

        private static double dMillisecondsSinceLastCacheClear = 0.0;
        private static int nCacheClearThresholdMilliseconds = 5000;
        private static double dMillisecondsSinceLastCache = 0.0;
        private static int nCacheThresholdMilliseconds = 2000;

        private static bool bPauseCaching = true;
        private static bool bCacheInProgress = false;
        private static bool bCleanupInProgress = false;

        public static void Draw(CanvasAnimatedDrawEventArgs args) {
            lock (Chunk.CacheLock) {
                for (int x = Camera.ChunkPositionX; x < Camera.ChunkPositionX + Chunk.MaxChunksVisibleX; x++) {
                    for (int y = Camera.ChunkPositionY; y < Camera.ChunkPositionY + Chunk.MaxChunksVisibleY; y++) {
                        Chunk c;
                        if (ChunkCache.TryGetValue(new PointInt(x, y), out c)) {
                            c.Draw(args);
                        }
                    }
                }
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
            long before = GC.GetTotalMemory(true);
            Chunk c = await Task.Run(() => Chunk.Create(_device, 0, 0));
            long after = GC.GetTotalMemory(true);
            GC.KeepAlive(c);
            Debug.ChunkSizeMB = "Chunk size (MB): " + ((after - before) / 1000000.0f).ToString("F") + "MB";
            lock (Chunk.CacheLock) {
                ChunkCache.Add(c.Coordinates, c);
            }

            c = await Task.Run(() => Chunk.Create(_device, 1, 0));
            lock (Chunk.CacheLock) {
                ChunkCache.Add(c.Coordinates, c);
            }

            Debug.AddTimedString("Creating initial chunks...", Colors.White);
            for (int i = -_cachedChunkLoadRadius; i <= _cachedChunkLoadRadius; i++) {
                for (int j = -_cachedChunkLoadRadius; j <= _cachedChunkLoadRadius; j++) {
                    if (i == 0 && j == 0) { continue; }
                    if (i == 1 && j == 0) { continue; }
                    await Task.Run(() => CacheChunk(i, j, bSuppressOutput: true));
                }
            }
            Debug.AddTimedString("Initial chunks created!", Colors.Green);
            bPauseCaching = false;
            await Debug.LogHeightmapValues();
        }

        public static bool CacheChunk(int x, int y, bool bSuppressOutput = false) {
            Chunk c;
            PointInt point = new PointInt(x, y);

            if (!ChunkCache.TryGetValue(point, out c)) {
                c = Chunk.Create(_device, x, y);
                Chunk temp;
                lock (Chunk.CacheLock) {
                    if (!ChunkCache.TryGetValue(point, out temp)) {
                        ChunkCache.Add(c.Coordinates, c);
                        if (!bSuppressOutput) {
                            Debug.AddTimedString("Caching: " + c.Coordinates.ToString(), Colors.White);
                        }
                    }
                }

                return true;
            }
            else {
                return false;
            }
        }

        public static void Cache() {
            if (bPauseCaching) { return; }
            if (bCacheInProgress) { Debug.AddTimedString("Last cache hasn't finished yet. Aborting...", Colors.Pink); return; }

            bCacheInProgress = true;
            int nChunksAdded = 0;
            Stopwatch s = Stopwatch.StartNew();

            Debug.AddTimedString("Updating cache...", Colors.Yellow);
            PointInt p = new PointInt(Camera.ChunkPositionX, Camera.ChunkPositionY);
            if (CacheChunk(p.X, p.Y)) { nChunksAdded++; }

            for (int i = -_cachedChunkLoadRadius; i <= Chunk.MaxChunksVisibleX + _cachedChunkLoadRadius; i++) {
                for (int j = -_cachedChunkLoadRadius; j <= Chunk.MaxChunksVisibleY + _cachedChunkLoadRadius; j++) {
                    if (i == 0 && j == 0) { continue; }
                    if (CacheChunk(p.X + i, p.Y + j, bSuppressOutput: true)) { nChunksAdded++; }
                }
            }

            s.Stop();
            Debug.AddTimedString("Cache update took " + s.ElapsedMilliseconds.ToString() + "ms", Colors.White);
            Debug.AddTimedString("Chunks added: " + nChunksAdded.ToString(), Colors.Green);
            bCacheInProgress = false;
        }

        public static void CacheCleanup() {
            if (bPauseCaching) { return; }
            if (bCleanupInProgress) { Debug.AddTimedString("Last cleanup not finished. Aborting...", Colors.Pink); }

            bCleanupInProgress = true;
            Stopwatch s = Stopwatch.StartNew();
            Debug.AddTimedString("Cleaning up cache...", Colors.Yellow);
            Dictionary<PointInt, Chunk> swap = new Dictionary<PointInt, Chunk>();
            lock (Chunk.CacheLock) {
                foreach (KeyValuePair<PointInt, Chunk> chunk in ChunkCache) {
                    if (
                        // depending on which side of the screen the chunk is relative to camera, unload threshold is different
                        // note that camera is anchored at top-left of screen
                        // need to account for on-screen chunks in addition to unload radius
                        ((chunk.Value.Coordinates.X > Camera.ChunkPositionX) && (chunk.Value.Coordinates.X - Camera.ChunkPositionX < _cachedChunkUnloadThreshold + Chunk.MaxChunksVisibleX))
                        || ((chunk.Value.Coordinates.X <= Camera.ChunkPositionX) && (Camera.ChunkPositionX - chunk.Value.Coordinates.X) < _cachedChunkUnloadThreshold)
                        &&
                        ((chunk.Value.Coordinates.Y > Camera.ChunkPositionY) && (chunk.Value.Coordinates.Y - Camera.ChunkPositionY) < _cachedChunkUnloadThreshold + Chunk.MaxChunksVisibleY)
                        || ((chunk.Value.Coordinates.Y <= Camera.ChunkPositionY) && (Camera.ChunkPositionY - chunk.Value.Coordinates.Y) < _cachedChunkUnloadThreshold)
                        ) {
                        swap.Add(chunk.Key, chunk.Value);
                    }
                    //&& (Math.Abs(Camera.ChunkPositionY - chunk.Value.Coordinates.Y) < _cachedChunkUnloadThreshold)) {
                }
            }
            int nChunksRemoved = ChunkCache.Count - swap.Count;
            lock (Chunk.CacheLock) {
                ChunkCache = swap;
            }
            s.Stop();
            Debug.AddTimedString("Cache cleanup took " + s.ElapsedMilliseconds.ToString() + "ms", Colors.White);
            Debug.AddTimedString("Chunks removed: " + nChunksRemoved.ToString(), Colors.Red);
            bCleanupInProgress = false;
        }

        public static int Elevation(int chunkX, int chunkY, int tileX, int tileY) {
            return ChunkCache[new PointInt(chunkX, chunkY)].Tiles[tileX, tileY].Elevation;
        }
    }
}