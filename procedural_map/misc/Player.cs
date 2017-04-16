using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace procedural_map {
    static class Player {
        private static DateTime LastMoveTime = DateTime.Now;
        private static float MoveThresholdMilliseconds = 75;

        #region "Position Members"

        #endregion

        // public static Vector2RowColumn PathEndPoint = null;
        public static Path Destination = null;

        public static void Update() {
            // keyboard movement
            // mouse movement

            TimeSpan TimeFromLastMove = DateTime.Now - LastMoveTime;
            if (TimeFromLastMove.TotalMilliseconds >= MoveThresholdMilliseconds) {
                if (Destination != null) {
                    LastMoveTime = DateTime.Now;
                    // Position = Destination.Nodes.Pop();

                    if (Destination.Nodes.Count == 0) { Destination = null; }
                }
            }
        }
    }
}