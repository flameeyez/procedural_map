using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace procedural_map {
    static class Mouse {
        private static double _x;
        public static double X {
            get { return _x; }
            set {
                _lastX = _x;
                _x = value;
            }
        }

        private static double _y;
        public static double Y {
            get { return _y; }
            set {
                _lastY = _y;
                _y = value;
            }
        }

        public static int ChunkX {
            get {
                int nReturn = (int)((X + Camera.PositionX) / Map.TILE_RESOLUTION);
                nReturn /= Chunk.ChunkSideLength;
                if (X + Camera.PositionX < 0) { nReturn--; }
                return nReturn;
            }
        }

        public static int ChunkY {
            get {
                int nReturn = (int)((Y + Camera.PositionY) / Map.TILE_RESOLUTION);
                nReturn /= Chunk.ChunkSideLength;
                if (Y + Camera.PositionY < 0) { nReturn--; }
                return nReturn;
            }
        }

        public static int TileX {
            get {
                int nReturn = (int)((X + Camera.PositionX) / Map.TILE_RESOLUTION);
                if (X + Camera.PositionX < 0) { nReturn--; }
                nReturn %= Chunk.ChunkSideLength;
                if (nReturn < 0) { nReturn += Chunk.ChunkSideLength; }
                return nReturn;
            }
        }

        public static int TileY {
            get {
                int nReturn = (int)((Y + Camera.PositionY) / Map.TILE_RESOLUTION);
                if (Y + Camera.PositionY < 0) { nReturn--; }
                nReturn %= Chunk.ChunkSideLength;
                if (nReturn < 0) { nReturn += Chunk.ChunkSideLength; }
                return nReturn;
            }
        }

        public static int AbsoluteTileX {
            get {
                int nReturn = (int)((X + Camera.PositionX) / Map.TILE_RESOLUTION);
                if (X + Camera.PositionX < 0) { nReturn--; }
                return nReturn;
            }
        }

        public static int AbsoluteTileY {
            get {
                int nReturn = (int)((Y + Camera.PositionY) / Map.TILE_RESOLUTION);
                if (Y + Camera.PositionY < 0) { nReturn--; }
                return nReturn;
            }
        }

        private static double _lastX;
        private static double _lastY;
        public static double DeltaX { get { return X - _lastX; } }
        public static double DeltaY { get { return Y - _lastY; } }

        public static string CoordinatesString { get { return "{" + X.ToString("F") + ", " + Y.ToString("F") + "}"; } }
        public static string ChunkString { get { return "{" + ChunkX.ToString() + ", " + ChunkY.ToString() + "}"; } }
        public static string TileString { get { return "{" + TileX.ToString() + ", " + TileY.ToString() + "}"; } }
        public static string AbsoluteTileString { get { return "{" + AbsoluteTileX.ToString() + ", " + AbsoluteTileY.ToString() + "}"; } }
        public static string ElevationString { get { return Map.Elevation(ChunkX, ChunkY, TileX, TileY).ToString(); } }

        public static bool LeftButtonDown { get; set; }
        public static bool RightButtonDown { get; set; }

        static Mouse() {
            LeftButtonDown = false;
            RightButtonDown = false;
        }

        public static void Draw(CanvasAnimatedDrawEventArgs args) {
            // screen position of TileX, TileY
            args.DrawingSession.DrawRectangle(new Windows.Foundation.Rect(AbsoluteTileX * Map.TILE_RESOLUTION - Camera.PositionX, AbsoluteTileY * Map.TILE_RESOLUTION - Camera.PositionY, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION), Colors.Red);
        }
    }
}
