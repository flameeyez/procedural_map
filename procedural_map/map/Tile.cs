using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace procedural_map {
    class Tile {
        // public static int MAX_TRAVERSABLE_ELEVATION = 20;
        // public int Elevation { get; set; }
        // public List<Event> Events = new List<Event>();
        public enum TILE_TYPE {
            MOUNTAINS,
            WATER,
            GRASS,
            GRASS_LIGHT,
            DESERT,
            FOREST
        }

        public TILE_TYPE TileType { get; set; }

        public Tile() {//int chunkCoordinateX, int chunkCoordinateY, int tileCoordinateX, int tileCoordinateY) {
            // TODO: replace with procedural elevation (global seed, noise)
            // Elevation = 0;
        }

        public bool IsTraversable() { return TileType != TILE_TYPE.MOUNTAINS && TileType != TILE_TYPE.WATER; }
    }
}
