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
                        args.DrawingSession.DrawImage(RenderTargetElevation,
                            new Rect(ScreenPositionX, ScreenPositionY, 
                                RenderTargetElevation.Bounds.Width * Map.TILE_RESOLUTION / Statics.OverworldTileResolution, 
                                RenderTargetElevation.Bounds.Height * Map.TILE_RESOLUTION / Statics.OverworldTileResolution));
                            //new Vector2((float)ScreenPositionX, (float)ScreenPositionY));
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
            chunk.RenderTargetElevation = new CanvasRenderTarget(device, ChunkSideLength * Statics.OverworldTileResolution, ChunkSideLength * Statics.OverworldTileResolution, 96);
            using (CanvasDrawingSession dsBackgroundColor = chunk.RenderTargetBackgroundColor.CreateDrawingSession()) {
                using (CanvasDrawingSession dsElevation = chunk.RenderTargetElevation.CreateDrawingSession()) {
                    for (int x = 0; x < ChunkSideLength; x++) {
                        for (int y = 0; y < ChunkSideLength; y++) {
                            // background color
                            dsBackgroundColor.FillRectangle(new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution), _debugBackgroundColor);
                            //dsBackgroundColor.DrawRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), Colors.Black);

                            // elevation
                            if (chunk.Tiles[x, y].Elevation < 70) {
                                dsElevation.DrawImage(Statics.BitmapOverworld,
                                    new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                    new Rect(Statics.OverworldTileWater.X, Statics.OverworldTileWater.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
                            }
                            else if(chunk.Tiles[x,y].Elevation < 80) {
                                dsElevation.DrawImage(Statics.BitmapOverworld,
                                    new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                    new Rect(Statics.OverworldTileDesert.X, Statics.OverworldTileDesert.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
                            }
                            else if (chunk.Tiles[x, y].Elevation < 100) {
                                dsElevation.DrawImage(Statics.BitmapOverworld,
                                    new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                    new Rect(Statics.OverworldTileGrassLight.X, Statics.OverworldTileGrassLight.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
                            }
                            else if (chunk.Tiles[x, y].Elevation < 130) {
                                dsElevation.DrawImage(Statics.BitmapOverworld,
                                    new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                    new Rect(Statics.OverworldTileGrass.X, Statics.OverworldTileGrass.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
                            }
                            else if (chunk.Tiles[x, y].Elevation < 150) {
                                dsElevation.DrawImage(Statics.BitmapOverworld,
                                    new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                    new Rect(Statics.OverworldTileForest.X, Statics.OverworldTileForest.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
                            }
                            else {
                                dsElevation.DrawImage(Statics.BitmapOverworld,
                                    new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                    new Rect(Statics.OverworldTileMountain.X, Statics.OverworldTileMountain.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
                            }

                            // args.DrawingSession.DrawImage(Statics.BitmapOverworld, new Rect(x, y, resolution, resolution), new Rect(Statics.OverworldTileForest.X, Statics.OverworldTileForest.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
                            //public static CanvasBitmap BitmapOverworld { get; set; }
                            //public static PointInt OverworldTileMountain = new PointInt(0, 0);
                            //public static PointInt OverworldTileGrass = new PointInt(32, 0);
                            //public static PointInt OverworldTileForest = new PointInt(64, 0);
                            //public static PointInt OverworldTileDesert = new PointInt(96, 0);
                            //public static PointInt OverworldTileWater = new PointInt(128, 0);
                            //public static PointInt OverworldTileGrassLight = new PointInt(160, 0);
                            //public static int OverworldTileResolution = 32;


                            //dsElevation.FillRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), Chunk.ElevationColor(chunk.Tiles[x, y].Elevation));
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