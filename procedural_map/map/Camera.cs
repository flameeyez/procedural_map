using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.System;
using Windows.UI;

namespace procedural_map {
    static class Camera {
        public static int PositionX { get; set; }
        public static int PositionY { get; set; }
        public static int AbsoluteTilePositionX {
            get {
                int nReturn = PositionX / Map.TILE_RESOLUTION;
                return PositionX < 0 ? nReturn - 1 : nReturn;
            }
        }
        public static int AbsoluteTilePositionY {
            get {
                int nReturn = PositionY / Map.TILE_RESOLUTION;
                return PositionY < 0 ? nReturn - 1 : nReturn;
            }
        }
        public static int ChunkTilePositionX { get; set; }
        public static int ChunkTilePositionY { get; set; }
        private static int _lastChunkPositionX;
        public static int ChunkPositionX { get; set; }
        private static int _lastChunkPositionY;
        public static int ChunkPositionY { get; set; }
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

        public static void Update(CanvasAnimatedUpdateEventArgs args) {
            UpdateChunkPositions();
            UpdateChunkTilePositions();
        }

        private static void UpdateChunkPositions() {
            int _currentChunkPositionX;
            if (AbsoluteTilePositionX < 0) { _currentChunkPositionX = AbsoluteTilePositionX / Chunk.ChunkSideLength - 1; }
            else { _currentChunkPositionX = AbsoluteTilePositionX / Chunk.ChunkSideLength; }
            _lastChunkPositionX = _currentChunkPositionX;
            ChunkPositionX = _currentChunkPositionX;

            int _currentChunkPositionY;
            if (AbsoluteTilePositionY < 0) { _currentChunkPositionY = AbsoluteTilePositionY / Chunk.ChunkSideLength - 1; }
            else { _currentChunkPositionY = AbsoluteTilePositionY / Chunk.ChunkSideLength; }
            _lastChunkPositionY = _currentChunkPositionY;
            ChunkPositionY = _currentChunkPositionY;
        }

        private static void UpdateChunkTilePositions() {
            int _chunkTilePositionX = AbsoluteTilePositionX % Chunk.ChunkSideLength;
            if (_chunkTilePositionX < 0) { _chunkTilePositionX += Chunk.ChunkSideLength; }
            ChunkTilePositionX = _chunkTilePositionX;

            int _chunkTilePositionY = AbsoluteTilePositionY % Chunk.ChunkSideLength;
            if (_chunkTilePositionY < 0) { _chunkTilePositionY += Chunk.ChunkSideLength; }
            ChunkTilePositionY = _chunkTilePositionY;
        }
    }
}
