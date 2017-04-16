using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace procedural_map {
    class Entity {
        private DateTime LastMoveTime = DateTime.Now;
        private float MoveDelayMilliseconds { get; set; } // = 75;
        public Path Destination = null;

        #region "Position Members"

        #endregion

        public Entity(float moveDelay) {
            MoveDelayMilliseconds = moveDelay;
        }

        public void Update() {
            TimeSpan TimeFromLastMove = DateTime.Now - LastMoveTime;
            if (TimeFromLastMove.TotalMilliseconds >= MoveDelayMilliseconds) {

                if (Destination == null) {
                    // Destination = Path.Create(map, Position, Globals.RandomPassableCoordinates(map, Position, 10));
                }

                if (Destination != null) {
                    // Position = Destination.Nodes.Pop();
                    LastMoveTime = DateTime.Now;
                    if (Destination.Nodes.Count == 0) { Destination = null; }
                }
            }
        }

        public void Draw(CanvasAnimatedDrawEventArgs args) {

        }

        public bool IsOnScreen() {
            return false;
        }
    }
}
