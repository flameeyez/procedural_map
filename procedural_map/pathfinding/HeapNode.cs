using System;

namespace procedural_map {
    class HeapNode {
        public float Key { get; set; }
        public PathNode Value { get; set; }

        public HeapNode(PathNode value) {
            Key = value.F;
            Value = value;
        }
    }
}

