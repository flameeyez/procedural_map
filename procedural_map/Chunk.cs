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

        public Point Coordinates { get; set; }
        public double PixelCoordinateX { get { return Coordinates.X * ChunkSideInPixels; } }
        public double PixelCoordinateY { get { return Coordinates.Y * ChunkSideInPixels; } }

        public double ScreenPositionX { get { return PixelCoordinateX - Camera.PositionX; } }
        public double ScreenPositionY { get { return PixelCoordinateY - Camera.PositionY; } }

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
            Coordinates = new Point(chunkCoordinateX, chunkCoordinateY);
        }

        public void Draw(CanvasAnimatedDrawEventArgs args) {
            if(IsOnScreen()) {
                Debug.OnScreenChunkCount++;
                args.DrawingSession.DrawImage(RenderTargetBackgroundColor, new Vector2((float)ScreenPositionX, (float)ScreenPositionY));
            }
        }

        public static Chunk Create(CanvasDevice device, int chunkCoordinateX, int chunkCoordinateY) {
            Stopwatch s = Stopwatch.StartNew();
            Chunk chunk = new Chunk(device, chunkCoordinateX, chunkCoordinateY);
            chunk.Tiles = new Tile[ChunkSideLength, ChunkSideLength];

            for(int tileX = 0; tileX < ChunkSideLength; tileX++) {
                for(int tileY = 0; tileY < ChunkSideLength; tileY++) {
                    chunk.Tiles[tileX, tileY] = new Tile(chunkCoordinateX, chunkCoordinateY, tileX, tileY);
                }
            }

            // draw chunk to render target
            Color _debugBackgroundColor = Statics.RandomColor();
            chunk.RenderTargetBackgroundColor = new CanvasRenderTarget(device, ChunkSideInPixels, ChunkSideInPixels, 96);
            //chunk.RenderTargetElevation = new CanvasRenderTarget(device, ChunkSideInPixels, ChunkSideInPixels, 96);
            using(CanvasDrawingSession dsBackgroundColor = chunk.RenderTargetBackgroundColor.CreateDrawingSession()) {
                //using(CanvasDrawingSession dsElevation = chunk.RenderTargetElevation.CreateDrawingSession()) {
                    for(int x = 0; x < ChunkSideLength; x++) {
                        for(int y = 0; y < ChunkSideLength; y++) {
                            // background color
                            dsBackgroundColor.FillRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), _debugBackgroundColor);
                            dsBackgroundColor.DrawRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), Colors.Black);

                            // elevation
                            // dsElevation.FillRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), chunk.Tiles[);
                        }
                    }
               // }
            }

            s.Stop();
            Debug.ChunkLoadTimes.Add(s.ElapsedMilliseconds);
            return chunk;
        }
    }
}