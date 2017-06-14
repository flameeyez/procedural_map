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
        private static int _cachedChunkLoadRadius = 2;//5;//3;
        private static int _cachedChunkUnloadThreshold = 2;//10;//5;
        public static int TILE_RESOLUTION = 32;//4;//32;

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

        public async static Task Initialize(CanvasDevice device) {
            _device = device;
            await Task.Run(() => Debug.DetermineChunkSizeInMB(device));
            await Task.Run(() => CacheInitialChunks());
            // await Debug.LogHeightmapValues();
        }

        public static async Task CacheInitialChunks() {
            Debug.AddTimedString("Creating initial chunks...", Colors.White);
            for (int x = 0; x < Chunk.MaxChunksVisibleX; x++) {
                for (int y = 0; y < Chunk.MaxChunksVisibleY; y++) {
                    await Task.Run(() => CacheChunk(new PointInt(x, y), bSuppressOutput: true));
                }
            }
            Debug.AddTimedString("Initial chunks created!", Colors.Green);
            bPauseCaching = false;
        }

        public static bool CacheChunk(PointInt coordinates, bool bSuppressOutput = false) {
            if (!ChunkCache.TryGetValue(coordinates, out Chunk c)) {
                c = Chunk.Create(_device, coordinates);
                lock (Chunk.CacheLock) {
                    if (!ChunkCache.TryGetValue(coordinates, out Chunk temp)) {
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

            PointInt coordinates = new PointInt(Camera.ChunkPositionX, Camera.ChunkPositionY);
            for (int i = -_cachedChunkLoadRadius; i <= Chunk.MaxChunksVisibleX + _cachedChunkLoadRadius; i++) {
                for (int j = -_cachedChunkLoadRadius; j <= Chunk.MaxChunksVisibleY + _cachedChunkLoadRadius; j++) {
                    if (CacheChunk(new PointInt(coordinates.X + i, coordinates.Y + j), bSuppressOutput: true)) {
                        // show debug message when caching first chunk
                        if (nChunksAdded == 0) { Debug.AddTimedString("Updating cache...", Colors.Yellow); }
                        nChunksAdded++;
                    }
                }
            }

            s.Stop();

            // report if chunks were added or if elapsed time > 0
            if (nChunksAdded > 0 || s.ElapsedMilliseconds > 0) {
                Debug.AddTimedString("Cache update took " + s.ElapsedMilliseconds.ToString() + "ms", Colors.White);
                Debug.AddTimedString("Chunks added: " + nChunksAdded.ToString(), Colors.Green);
            }

            bCacheInProgress = false;
        }

        public static void CacheCleanup() {
            if (bPauseCaching) { return; }
            if (bCleanupInProgress) { Debug.AddTimedString("Last cleanup not finished. Aborting...", Colors.Pink); return; }

            bool bCleanupPromptShown = false;
            bCleanupInProgress = true;
            Stopwatch s = Stopwatch.StartNew();

            Dictionary<PointInt, Chunk> swap = new Dictionary<PointInt, Chunk>();
            lock (Chunk.CacheLock) {
                foreach (KeyValuePair<PointInt, Chunk> chunk in ChunkCache) {
                    if ((chunk.Value.Coordinates.X < Camera.ChunkPositionX - _cachedChunkUnloadThreshold)
                     || (chunk.Value.Coordinates.X > Camera.ChunkPositionX + _cachedChunkUnloadThreshold + Chunk.MaxChunksVisibleX)
                     || (chunk.Value.Coordinates.Y < Camera.ChunkPositionY - _cachedChunkUnloadThreshold)
                     || (chunk.Value.Coordinates.Y > Camera.ChunkPositionY + _cachedChunkUnloadThreshold + Chunk.MaxChunksVisibleY)) {
                        // show debug message when removing first chunk
                        if (!bCleanupPromptShown) {
                            bCleanupPromptShown = true;
                            Debug.AddTimedString("Cleaning up cache...", Colors.Yellow);
                        }
                        continue;
                    }
                    swap.Add(chunk.Key, chunk.Value);
                }
            }

            int nChunksRemoved = ChunkCache.Count - swap.Count;

            lock (Chunk.CacheLock) {
                ChunkCache = swap;
            }

            s.Stop();

            // report if chunks were removed or if elapsed time > 0
            if (nChunksRemoved > 0 || s.ElapsedMilliseconds > 0) {
                Debug.AddTimedString("Cache cleanup took " + s.ElapsedMilliseconds.ToString() + "ms", Colors.White);
                Debug.AddTimedString("Chunks removed: " + nChunksRemoved.ToString(), Colors.Red);
            }

            bCleanupInProgress = false;
        }

        public static Tile.TILE_TYPE TileType(int chunkX, int chunkY, int tileX, int tileY) {
            return ChunkCache[new PointInt(chunkX, chunkY)].Tiles[tileX, tileY].TileType;
        }
    }
}