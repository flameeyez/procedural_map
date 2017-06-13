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
        public CanvasRenderTarget RenderTargetTerrain { get; set; }

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

        public Chunk(CanvasDevice device, PointInt coordinates) {
            Coordinates = coordinates;
        }

        public void Draw(CanvasAnimatedDrawEventArgs args) {
            if (IsOnScreen()) {
                Debug.OnScreenChunkCount++;
                switch (Debug.DrawMode) {
                    case Debug.DRAW_MODE.BACKGROUND_COLOR:
                        args.DrawingSession.DrawImage(RenderTargetBackgroundColor,
                            new Rect(ScreenPositionX, ScreenPositionY,
                                RenderTargetBackgroundColor.Bounds.Width * Map.TILE_RESOLUTION,
                                RenderTargetBackgroundColor.Bounds.Height * Map.TILE_RESOLUTION));
                        break;
                    case Debug.DRAW_MODE.TERRAIN:
                        args.DrawingSession.DrawImage(RenderTargetTerrain,
                            new Rect(ScreenPositionX, ScreenPositionY,
                                RenderTargetTerrain.Bounds.Width * Map.TILE_RESOLUTION / Statics.OverworldTileResolution,
                                RenderTargetTerrain.Bounds.Height * Map.TILE_RESOLUTION / Statics.OverworldTileResolution));
                        //new Vector2((float)ScreenPositionX, (float)ScreenPositionY));
                        break;
                }
            }
        }

        public static Chunk Create(CanvasDevice device, PointInt coordinates) {
            Stopwatch s = Stopwatch.StartNew();
            Chunk chunk = new Chunk(device, coordinates);
            chunk.Tiles = new Tile[ChunkSideLength, ChunkSideLength];

            for (int tileX = 0; tileX < ChunkSideLength; tileX++) {
                for (int tileY = 0; tileY < ChunkSideLength; tileY++) {
                    chunk.Tiles[tileX, tileY] = new Tile();// chunkCoordinateX, chunkCoordinateY, tileX, tileY);
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
            chunk.RenderTargetBackgroundColor = new CanvasRenderTarget(device, ChunkSideLength, ChunkSideLength, 96);
            chunk.RenderTargetTerrain = new CanvasRenderTarget(device, ChunkSideLength * Statics.OverworldTileResolution, ChunkSideLength * Statics.OverworldTileResolution, 96);
            //chunk.RenderTargetElevation = new CanvasRenderTarget(device, ChunkSideLength * Statics.OverworldTileResolution, ChunkSideLength * Statics.OverworldTileResolution, 96);
            using (CanvasDrawingSession dsBackgroundColor = chunk.RenderTargetBackgroundColor.CreateDrawingSession()) {
                using (CanvasDrawingSession dsElevation = chunk.RenderTargetTerrain.CreateDrawingSession()) {
                    for (int x = 0; x < ChunkSideLength; x++) {
                        for (int y = 0; y < ChunkSideLength; y++) {
                            // background color
                            dsBackgroundColor.FillRectangle(new Rect(x, y, 1, 1), _debugBackgroundColor);
                            //dsBackgroundColor.DrawRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), Colors.Black);

                            // elevation
                            switch (chunk.Tiles[x, y].TileType) {
                                case Tile.TILE_TYPE.GRASS:
                                    dsElevation.DrawImage(Statics.BitmapOverworld,
                                        new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                        new Rect(Statics.OverworldTileGrass.X, Statics.OverworldTileGrass.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));

                                    // dsElevation.FillRectangle(new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution), Colors.Green);
                                    break;
                                case Tile.TILE_TYPE.MOUNTAINS:
                                    dsElevation.DrawImage(Statics.BitmapOverworld,
                                        new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                        new Rect(Statics.OverworldTileMountain.X, Statics.OverworldTileMountain.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));

                                    // dsElevation.FillRectangle(new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution), Colors.Brown);
                                    break;
                                case Tile.TILE_TYPE.WATER:
                                    dsElevation.DrawImage(Statics.BitmapOverworld,
                                        new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                        new Rect(Statics.OverworldTileWater.X, Statics.OverworldTileWater.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));

                                    // dsElevation.FillRectangle(new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution), Colors.Blue);
                                    break;
                                case Tile.TILE_TYPE.FOREST:
                                    dsElevation.DrawImage(Statics.BitmapOverworld,
                                        new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                        new Rect(Statics.OverworldTileForest.X, Statics.OverworldTileForest.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));

                                    // dsElevation.FillRectangle(new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution), Colors.DarkGreen);
                                    break;
                                case Tile.TILE_TYPE.DESERT:
                                    dsElevation.DrawImage(Statics.BitmapOverworld,
                                        new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                        new Rect(Statics.OverworldTileDesert.X, Statics.OverworldTileDesert.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));

                                    // dsElevation.FillRectangle(new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution), Colors.DarkGreen);
                                    break;
                                case Tile.TILE_TYPE.GRASS_LIGHT:
                                    dsElevation.DrawImage(Statics.BitmapOverworld,
                                        new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
                                        new Rect(Statics.OverworldTileGrassLight.X, Statics.OverworldTileGrassLight.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));

                                    // dsElevation.FillRectangle(new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution), Colors.DarkGreen);
                                    break;
                            }
                            //dsElevation.FillRectangle(new Rect(x * Statics.OverworldTileResolution, y * Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution), Chunk.ElevationColor(chunk.Tiles[x, y].Elevation));
                            //dsElevation.DrawRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), Colors.Black);
                        }
                    }
                }
            }

            s.Stop();
            lock (Debug.DebugCollectionsLock) { Debug.ChunkLoadTimes.Add(s.ElapsedMilliseconds); }
            return chunk;
        }

        public void GenerateHeightMap() {
            for (int x = 0; x < ChunkSideLength; x++) {
                for (int y = 0; y < ChunkSideLength; y++) {
                    double scale = 0.02;

                    double offsetMountains = 12345678;
                    double offsetWater = 82738492;
                    double offsetForest = 23498758;
                    double offsetDesert = 73829178;
                    double offsetGrassLight = 38291731;

                    // octave use
                    double xinBase = (x + Coordinates.X * ChunkSideLength);
                    double yinBase = (y + Coordinates.Y * ChunkSideLength);

                    // mountain layer
                    double dMountains = SimplexNoise.Sum2D(xinBase + offsetMountains, yinBase + offsetMountains, octaves: 16, persistence: 0.5, frequency: scale);
                    dMountains *= 100;
                    dMountains += 100;

                    // if mountain exceeds threshold value, use it and continue
                    if (dMountains > 130) {
                        Tiles[x, y].TileType = Tile.TILE_TYPE.MOUNTAINS;
                        continue;
                    }

                    // water layer
                    double dWater = SimplexNoise.Sum2D(xinBase + offsetWater, yinBase + offsetWater, octaves: 16, persistence: 0.5, frequency: scale);
                    dWater *= 100;
                    dWater += 100;

                    if (dWater > 130) {
                        Tiles[x, y].TileType = Tile.TILE_TYPE.WATER;
                        continue;
                    }

                    // forest layer
                    double dForest = SimplexNoise.Sum2D(xinBase + offsetForest, yinBase + offsetForest, octaves: 16, persistence: 0.5, frequency: scale);
                    dForest *= 100;
                    dForest += 100;

                    if (dForest > 130) {
                        Tiles[x, y].TileType = Tile.TILE_TYPE.FOREST;
                        continue;
                    }

                    // desert layer
                    double dDesert = SimplexNoise.Sum2D(xinBase + offsetDesert, yinBase + offsetDesert, octaves: 16, persistence: 0.5, frequency: scale);
                    dDesert *= 100;
                    dDesert += 100;

                    if (dDesert > 130) {
                        Tiles[x, y].TileType = Tile.TILE_TYPE.DESERT;
                        continue;
                    }

                    // light grass layer
                    double dGrassLight = SimplexNoise.Sum2D(xinBase + offsetGrassLight, yinBase + offsetGrassLight, octaves: 16, persistence: 0.5, frequency: scale);
                    dGrassLight *= 100;
                    dGrassLight += 100;

                    if (dGrassLight > 130) {
                        Tiles[x, y].TileType = Tile.TILE_TYPE.GRASS_LIGHT;
                        continue;
                    }

                    // default to grass
                    Tiles[x, y].TileType = Tile.TILE_TYPE.GRASS;

                    // no octave use
                    //double xin = (x + Coordinates.X * ChunkSideLength) * scale;
                    //double yin = (y + Coordinates.Y * ChunkSideLength) * scale;
                    //double d = SimplexNoise.Noise2D(xin, yin);

                    //Tiles[x, y].Elevation = n; // 0-200
                    //if (Debug.HeightmapValues.Count < 1000) {
                    //    Debug.HeightmapValues.Add(new Tuple<double, double, int>(x, y, n));
                    //}
                }
            }
        }
    }
}

//if (chunk.Tiles[x, y].Elevation<Statics.ElevationThresholdWater) {
//    dsElevation.DrawImage(Statics.BitmapOverworld,
//        new Rect(x* Statics.OverworldTileResolution, y* Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
//        new Rect(Statics.OverworldTileWater.X, Statics.OverworldTileWater.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
//}
//else if(chunk.Tiles[x, y].Elevation<Statics.ElevationThresholdDesert) {
//    dsElevation.DrawImage(Statics.BitmapOverworld,
//        new Rect(x* Statics.OverworldTileResolution, y* Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
//        new Rect(Statics.OverworldTileDesert.X, Statics.OverworldTileDesert.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
//}
//else if (chunk.Tiles[x, y].Elevation<Statics.ElevationThresholdGrassLight) {
//    dsElevation.DrawImage(Statics.BitmapOverworld,
//        new Rect(x* Statics.OverworldTileResolution, y* Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
//        new Rect(Statics.OverworldTileGrassLight.X, Statics.OverworldTileGrassLight.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
//}
//else if (chunk.Tiles[x, y].Elevation<Statics.ElevationThresholdGrass) {
//    dsElevation.DrawImage(Statics.BitmapOverworld,
//        new Rect(x* Statics.OverworldTileResolution, y* Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
//        new Rect(Statics.OverworldTileGrass.X, Statics.OverworldTileGrass.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
//}
//else if (chunk.Tiles[x, y].Elevation<Statics.ElevationThresholdForest) {
//    dsElevation.DrawImage(Statics.BitmapOverworld,
//        new Rect(x* Statics.OverworldTileResolution, y* Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
//        new Rect(Statics.OverworldTileForest.X, Statics.OverworldTileForest.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
//}
//else {
//    dsElevation.DrawImage(Statics.BitmapOverworld,
//        new Rect(x* Statics.OverworldTileResolution, y* Statics.OverworldTileResolution, Statics.OverworldTileResolution, Statics.OverworldTileResolution),
//        new Rect(Statics.OverworldTileMountain.X, Statics.OverworldTileMountain.Y, Statics.OverworldTileResolution, Statics.OverworldTileResolution));
//}