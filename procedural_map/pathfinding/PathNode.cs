using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace procedural_map {
    class PathNode {
        public Vector2RowColumn Coordinates;
        public PathNode ParentNode;

        public float G;
        public float H;
        public float F { get { return G + H; } }

        public PathNode(Vector2RowColumn coords, PathNode parentNode, Vector2RowColumn destination) {
            Coordinates = coords;
            ParentNode = parentNode;

            CalculateG();
            CalculateH(destination);
        }

        public PathNode(int row, int column, PathNode parentNode, Vector2RowColumn destination) {
            Coordinates = new Vector2RowColumn(row, column);
            ParentNode = parentNode;

            CalculateG();
            CalculateH(destination);
        }

        public void CalculateG() {
            if (ParentNode == null) { return; }

            G = ParentNode.G + (IsDiagonalToParent() ? 1.4f : 1.0f);
        }

        public void CalculateH(Vector2RowColumn destination) {
            H = Math.Abs(Coordinates.Row - destination.Row);
            H += Math.Abs(Coordinates.Column - destination.Column);
        }

        private bool IsDiagonalToParent() {
            return (ParentNode.Coordinates.Row != Coordinates.Row) && (ParentNode.Coordinates.Column != Coordinates.Column);
        }
    }
}