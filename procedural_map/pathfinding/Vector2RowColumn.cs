using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace procedural_map {
    class Vector2RowColumn {
        public int Row { get; set; }
        public int Column { get; set; }

        public Vector2RowColumn(int row, int column) {
            Row = row;
            Column = column;
        }

        public override bool Equals(Object obj) {
            Vector2RowColumn vobj = (Vector2RowColumn)obj;
            return (this.Column == vobj.Column) && (this.Row == vobj.Row);
        }

        public override int GetHashCode() {
            return (Row.GetHashCode() << 5) + Column.GetHashCode();
        }
    }
}
