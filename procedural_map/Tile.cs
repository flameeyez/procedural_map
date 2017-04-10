using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace procedural_map {
    class Tile {
        public static int MAX_TRAVERSABLE_ELEVATION = 20;
        public int Elevation { get; set; }
        // public List<Event> Events = new List<Event>();

        public Tile(int chunkCoordinateX, int chunkCoordinateY, int tileCoordinateX, int tileCoordinateY) {
            // TODO: replace with procedural elevation (global seed, noise)
            Elevation = 1;
        }

        public bool IsTraversable() { return Elevation > 0 && Elevation <= MAX_TRAVERSABLE_ELEVATION; }
    }
}
