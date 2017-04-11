using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.System;

namespace procedural_map {
    static class Camera {
        public static double PositionX { get; set; }
        public static double PositionY { get; set; }
        public static int AbsoluteTilePositionX { get { return (int)(Math.Floor(PositionX / Map.TILE_RESOLUTION)); } }
        public static int AbsoluteTilePositionY { get { return (int)(Math.Floor(PositionY / Map.TILE_RESOLUTION)); } }
        public static int ChunkTilePositionX {
            get {
                int nReturn = AbsoluteTilePositionX % Chunk.ChunkSideLength;
                if(nReturn < 0) { nReturn += Chunk.ChunkSideLength; }
                return nReturn;
            }
        }
        public static int ChunkTilePositionY {
            get {
                int nReturn = AbsoluteTilePositionY % Chunk.ChunkSideLength;
                if (nReturn < 0) { nReturn += Chunk.ChunkSideLength; }
                return nReturn;
            }
        }
        public static int ChunkPositionX {
            get {
                if (AbsoluteTilePositionX < 0) {
                    return AbsoluteTilePositionX / Chunk.ChunkSideLength - 1;
                }
                else {
                    return AbsoluteTilePositionX / Chunk.ChunkSideLength;
                }
            }
        }
        public static int ChunkPositionY {
            get {
                if (AbsoluteTilePositionY < 0) {
                    return AbsoluteTilePositionY / Chunk.ChunkSideLength - 1;
                }
                else {
                    return AbsoluteTilePositionY / Chunk.ChunkSideLength;
                }
            }
        }
        private static int _velocity = 15;

        internal static void KeyDown(VirtualKey vk) {
            switch (vk) {
                case VirtualKey.Up:
                    PositionY -= _velocity;
                    break;
                case VirtualKey.Down:
                    PositionY += _velocity;
                    break;
                case VirtualKey.Left:
                    PositionX -= _velocity;
                    break;
                case VirtualKey.Right:
                    PositionX += _velocity;
                    break;
            }
        }

        public static string CoordinatesString() {
            StringBuilder sb = new StringBuilder("{");
            sb.Append(PositionX.ToString("F"));
            sb.Append(", ");
            sb.Append(PositionY.ToString("F"));
            sb.Append("}");
            return sb.ToString();
        }

        public static string ChunkTilePositionString() {
            StringBuilder sb = new StringBuilder("{");
            sb.Append(ChunkTilePositionX.ToString());
            sb.Append(", ");
            sb.Append(ChunkTilePositionY.ToString());
            sb.Append("}");
            return sb.ToString();
        }

        public static string ChunkPositionString() {
            StringBuilder sb = new StringBuilder("{");
            sb.Append(ChunkPositionX.ToString());
            sb.Append(", ");
            sb.Append(ChunkPositionY.ToString());
            sb.Append("}");
            return sb.ToString();
        }
    }
}
