using System;
using System.Collections.Generic;

namespace procedural_map {
    class PathNodeCollection {
        public List<PathNode> Nodes;

        public PathNodeCollection() {
            Nodes = new List<PathNode>();
        }

        public bool Contains(int row, int column) {
            foreach (PathNode node in Nodes) {
                if (node.Coordinates.Row == row && node.Coordinates.Column == column) {
                    return true;
                }
            }

            return false;
        }

        public void Add(PathNode node) {
            Nodes.Add(node);
        }
    }
}

