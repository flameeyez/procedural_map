using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace procedural_map {
    class Path {
        public DateTime DebugCreationTime;

        public int OpenSetMaximumCount { get; set; }

        public Vector2RowColumn Start;
        public Vector2RowColumn Destination;
        public int InsertCount = 0;
        public int Clashes = 0;
        public BinaryHeap OpenSet;
        public PathNodeCollection ClosedSet;
        public List<Vector2RowColumn> DebugPath = new List<Vector2RowColumn>();
        public TimeSpan TimeToCreate;

        // add nodes in reverse order
        public Stack<Vector2RowColumn> Nodes = null;

        public Path() {
            DebugCreationTime = DateTime.Now;
            Nodes = new Stack<Vector2RowColumn>();
        }

        public static Path Create(Vector2RowColumn start, Vector2RowColumn destination) {
            DateTime StartTime = DateTime.Now;
            TimeSpan ElapsedTime = TimeSpan.Zero;

            if (destination == null) { return null; }
            if (start.Equals(destination)) { return null; }
            
            // if (map.IsImpassable(destination.Row, destination.Column)) { return null; }

            // DEBUG
            Path returnPath = new Path();
            DateTime CreationTime = DateTime.Now;

            returnPath.Start = start;
            returnPath.Destination = destination;
            returnPath.OpenSet = new BinaryHeap(500);
            returnPath.ClosedSet = new PathNodeCollection();

            PathNode startingNode = new PathNode(start, null, destination);
            //DebugInsertCount++;
            returnPath.OpenSet.Insert(startingNode);

            while (returnPath.OpenSet.CurrentSize > 0) {
                ElapsedTime = DateTime.Now - StartTime;
                if (ElapsedTime.TotalMilliseconds > 200) {
                    return null;
                }

                // TODO: fix maximum heap size
                //if (returnPath.OpenSet.CurrentSize >= 500) {
                //    // bail on large OpenSets?
                //    return null;
                //}

                PathNode currentNode = returnPath.OpenSet.GetRootValue();
                if (currentNode.Coordinates.Equals(destination)) {
                    // destination reached; build stack
                    // Path path = new Path();
                    while (currentNode.ParentNode != null) {
                        // path.Nodes.Push(currentNode.Coordinates);
                        returnPath.DebugPath.Add(currentNode.Coordinates);
                        returnPath.Nodes.Push(currentNode.Coordinates);
                        currentNode = currentNode.ParentNode;
                    }

                    returnPath.TimeToCreate = DateTime.Now - CreationTime;

                    return returnPath;
                }

                returnPath.ClosedSet.Add(returnPath.OpenSet.RemoveRoot().Value);

                for (int row = currentNode.Coordinates.Row - 1; row <= currentNode.Coordinates.Row + 1; row++) {
                    // TODO: fix this?
                    // if (row < 0 || row >= map.GrassLayer.Tiles.GetLength(0)) { continue; }

                    for (int column = currentNode.Coordinates.Column - 1; column <= currentNode.Coordinates.Column + 1; column++) {
                        // TODO: fix this?
                        // if (column < 0 || column >= map.GrassLayer.Tiles.GetLength(1)) { continue; }
                        
                        // if (map.IsImpassable(row, column)) { continue; }
                        if (returnPath.ClosedSet.Contains(row, column)) { continue; }

                        // valid tile
                        PathNode newNode = new PathNode(row, column, currentNode, destination);
                        // check if exists node in OpenSet with current coordinates
                        PathNode compareNode = returnPath.OpenSet.GetNode(newNode.Coordinates);
                        if (compareNode != null) {
                            // TODO: fix?
                            returnPath.Clashes++;
                            // if(returnPath.Clashes++ > 200) { return null; }

                            if (newNode.G < compareNode.G) {
                                compareNode.ParentNode = newNode.ParentNode;
                                compareNode.CalculateG();
                                compareNode.CalculateH(destination);
                            }
                        }
                        else {
                            // no match found; add new node
                            returnPath.OpenSet.Insert(newNode);
                        }
                    }
                }
            }

            // OpenSet is empty; no path found
            return null;
        }
    }
}


////////////////
//
// DEBUG
//returnPath.OpenSetMaximumCount = returnPath.OpenSet.MaximumCount;
//
////////////////
