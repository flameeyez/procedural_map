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
        public static int ChunkSideLength = 100;
        public static int ChunkSideInPixels { get { return ChunkSideLength * Map.TILE_RESOLUTION; } }
        public static int MaxChunksVisibleX = Statics.ClientWidth / ChunkSideInPixels + 1;
        public static int MaxChunksVisibleY = Statics.ClientHeight / ChunkSideInPixels + 1;

        public CanvasRenderTarget RenderTarget { get; set; }

        public int ChunkCoordinateX { get; set; }
        public int ChunkCoordinateY { get; set; }
        public long PixelCoordinateX { get { return ChunkCoordinateX * ChunkSideInPixels; } }
        public long PixelCoordinateY { get { return ChunkCoordinateY * ChunkSideInPixels; } }

        public long RelativePositionX { get { return PixelCoordinateX - Camera.PositionX; } }
        public long RelativePositionY { get { return PixelCoordinateY - Camera.PositionY; } }

        public Tile[,] Tiles;
        public bool IsOnScreen() {
            return (RelativePositionX + ChunkSideInPixels - 1 >= 0 && RelativePositionX < Statics.ClientWidth)
                && (RelativePositionY + ChunkSideInPixels - 1 >= 0 && RelativePositionY < Statics.ClientHeight);
        }

        public Chunk(CanvasDevice device, int chunkCoordinateX, int chunkCoordinateY) {
            ChunkCoordinateX = chunkCoordinateX;
            ChunkCoordinateY = chunkCoordinateY;
        }

        public void Draw(CanvasAnimatedDrawEventArgs args) {
            if (IsOnScreen()) {
                Debug.OnScreenChunkCount++;
                args.DrawingSession.DrawImage(RenderTarget, new Vector2(RelativePositionX, RelativePositionY));
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

            // draw chunk to render target
            Color _debugBackgroundColor = Statics.RandomColor();
            chunk.RenderTarget = new CanvasRenderTarget(device, ChunkSideInPixels, ChunkSideInPixels, 96);
            using (CanvasDrawingSession ds = chunk.RenderTarget.CreateDrawingSession()) {
                for (int x = 0; x < ChunkSideLength; x++) {
                    for (int y = 0; y < ChunkSideLength; y++) {
                        // TODO: replace with elevation color/tile
                        ds.FillRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), _debugBackgroundColor);
                        ds.DrawRectangle(new Rect(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), Colors.Black);
                        // ds.DrawText(chunk.ChunkCoordinateX.ToString() + "," + chunk.ChunkCoordinateY.ToString(), new Vector2(x * Map.TILE_RESOLUTION, y * Map.TILE_RESOLUTION), Colors.White);
                    }
                }
            }

            s.Stop();
            Debug.ChunkLoadTimes.Add(s.ElapsedMilliseconds);
            return chunk;
        }
    }
}