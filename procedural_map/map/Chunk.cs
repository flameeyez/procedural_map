using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;


namespace procedural_map {
    class Chunk {
        public static object CacheLock = new object();
        public static int ChunkSideLength = 50;
        public static int ChunkSideInPixels { get { return ChunkSideLength * Map.TILE_RESOLUTION; } }
        private static int _maxChunksVisibleX;
        public static int MaxChunksVisibleX { get { return _maxChunksVisibleX; } }
        private static int _maxChunksVisibleY;
        public static int MaxChunksVisibleY { get { return _maxChunksVisibleY; } }

        public CanvasRenderTarget RenderTargetBackgroundColor { get; set; }
        public CanvasRenderTarget RenderTargetElevation { get; set; }

        public PointInt Coordinates { get; set; }
        public int PixelCoordinateX { get { return Coordinates.X * ChunkSideInPixels; } }
        public int PixelCoordinateY { get { return Coordinates.Y * ChunkSideInPixels; } }

        public int ScreenPositionX { get { return PixelCoordinateX - Camera.PositionX; } }
        public int ScreenPositionY { get { return PixelCoordinateY - Camera.PositionY; } }

        public Tile[,] Tiles;
        public bool IsOnScreen() {
            return (ScreenPositionX + ChunkSideInPixels - 1 >= 0 && ScreenPositionX < Statics.ClientWidth)
                && (ScreenPositionY + ChunkSideInPixels - 1 >= 0 && ScreenPositionY < Statics.ClientHeight);
        }

        static Chunk() {
            _maxChunksVisibleX = (int)Math.Ceiling(Statics.ClientWidth / ChunkSideInPixels) + 1;
            _maxChunksVisibleY = (int)Math.Ceiling(Statics.ClientHeight / ChunkSideInPixels) + 1;
        }

        public Chunk(CanvasDevice device, int chunkCoordinateX, int chunkCoordinateY) {
            Coordinates = new PointInt(chunkCoordinateX, chunkCoordinateY);
        }

        public void Draw(CanvasAnimatedDrawEventArgs args) {
            if (IsOnScreen()) {
                Debug.OnScreenChunkCount++;
                switch (Debug.DrawMode) {
                    case Debug.DRAW_MODE.BACKGROUND_COLOR:
                        args.DrawingSession.DrawImage(RenderTargetBackgroundColor, new Vector2((float)ScreenPositionX, (float)ScreenPositionY));
                        break;
                    case Debug.DRAW_MODE.ELEVATION:
                        args.DrawingSession.DrawImage(RenderTargetElevation, new Vector2((float)ScreenPositionX, (float)ScreenPositionY));
                        break;
                }
            }
        }

        public static Chunk Create(CanvasDevice device, int chunkCoordinateX, int chunkCoordinateY) {
            Stopwatch s = Stopwatch.StartNew();
            Chunk chunk = new Chunk(device, chunkCoordinateX, chunkCoordinateY);
            chunk.Tiles = new Tile[ChunkSideLength, ChunkSideLength];

            for (int tileX = 0; tileX < ChunkSideLength; tileX++) {
                for (int tileY = 0; tileY < ChunkSideLength; tileY++) {
                    chunk.Tiles[tileX, tileY] = new Tile(chunkCoordinateX, chunkCoordinateY, tileX, tileY);
                }
            }

            Stopwatch sHeightMap = Stopwatch.StartNew();
            chunk.GenerateHeightMap();
            sHeightMap.Stop();

            lock (Debug.DebugCollectionsLock) {
                Debug.HeightMapTimes.Add(sHeightMap.ElapsedMilliseconds);
            }

            // draw chunk to render target
            Color _debugBackgroundColor = Statics.RandomColor();
            chunk.RenderTargetBackgroundColor = new CanvasRenderTarget(device, ChunkSideInPixels, ChunkSideInPixels, 96);
            chunk.RenderTargetElevation = new CanvasRenderTarget(device, ChunkSideInPixels, ChunkSideInPixels, 96);
            using (CanvasDrawingSession dsBackgroundColor = chunk.RenderTargetBackgroundColor.CreateDrawingSession()) {
                using (CanvasDrawingSession dsElevation = chunk.RenderTargetElevation.CreateDrawingSession()) {
                    for (int x = 0; x < ChunkSideLength; x++) {
                        for (int y = 0; y < ChunkSideLength; y++) {
                            // background color
                            dsBackgroundColor.FillRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), _debugBackgroundColor);
                            //dsBackgroundColor.DrawRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), Colors.Black);

                            // elevation
                            dsElevation.FillRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), Chunk.ElevationColor(chunk.Tiles[x, y].Elevation));
                            //dsElevation.DrawRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), Colors.Black);
                        }
                    }
                }
            }

            s.Stop();
            lock (Debug.DebugCollectionsLock) { Debug.ChunkLoadTimes.Add(s.ElapsedMilliseconds); }
            return chunk;
        }

        public static Color ElevationColor(int elevation) {
            int e = elevation + 50;
            if (e > 255) { e = 255; }
            return Color.FromArgb(255, 0, (byte)e, 0);

            // TODO: replace green gradient with actual height interpretation; possible replace this mechanism completely
            //if (elevation < 1) { return Colors.Blue; }
            //else if (elevation < 2) { return Colors.Gold; }
            //else if (elevation < 10) { return Colors.Green; }
            //else if (elevation < 15) { return Colors.DarkGreen; }
            //else if (elevation < 27) { return Colors.Green; }
            //else if (elevation < 30) { return Colors.Brown; }
            //else { return Colors.White; }
        }

        public void GenerateHeightMap() {
            for (int x = 0; x < ChunkSideLength; x++) {
                for (int y = 0; y < ChunkSideLength; y++) {
                    double scale = 0.02;

                    // octave use
                    double xin = (x + Coordinates.X * ChunkSideLength);
                    double yin = (y + Coordinates.Y * ChunkSideLength);
                    double d = SimplexNoise.Sum2D(xin, yin, octaves: 16, persistence: 0.5, frequency: scale);

                    // no octave use
                    //double xin = (x + Coordinates.X * ChunkSideLength) * scale;
                    //double yin = (y + Coordinates.Y * ChunkSideLength) * scale;
                    //double d = SimplexNoise.Noise2D(xin, yin);

                    d *= 100;
                    d += 100;
                    int n = (int)d;
                    Tiles[x, y].Elevation = n; // 0-200
                    if (Debug.HeightmapValues.Count < 1000) {
                        Debug.HeightmapValues.Add(new Tuple<double, double, int>(x, y, n));
                    }
                }
            }
        }
    }
}