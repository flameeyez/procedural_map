using System;
using System.Collections;
using System.Collections.Generic;

namespace procedural_map {
    class BinaryHeap {
        private Hashtable CoordinatesHashTable;
        public HeapNode[] heapArray;
        private int maxSize;

        public int CurrentSize { get; set; }
        public int MaximumCount { get; set; }

        public BinaryHeap(int maxHeapSize) {
            maxSize = maxHeapSize;
            CurrentSize = 0;
            MaximumCount = 0;
            heapArray = new HeapNode[maxSize];
            CoordinatesHashTable = new Hashtable();
        }

        public bool IsEmpty() {
            return CurrentSize == 0;
        }

        public bool Insert(PathNode node) {
            if (CurrentSize == maxSize) { return false; }
            if (CurrentSize > MaximumCount) { MaximumCount = CurrentSize; }

            // HeapNode newNode = new HeapNode(node);
            heapArray[CurrentSize] = new HeapNode(node); //newNode;

            CoordinatesHashTable.Add(heapArray[CurrentSize].Value.Coordinates, heapArray[CurrentSize].Value); // newNode.Value.Coordinates, newNode);

            CascadeUp(CurrentSize++);

            return true;
        }

        public void CascadeUp(int index) {
            int parent = (index - 1) / 2;
            HeapNode bottom = heapArray[index];
            while (index > 0 && heapArray[parent].Key > bottom.Key) {
                heapArray[index] = heapArray[parent];
                index = parent;
                parent = (parent - 1) / 2;
            }
            heapArray[index] = bottom;
        }

        // problematic without garbage collection
        public HeapNode RemoveRoot() {
            if (CurrentSize == 0) { return null; }

            HeapNode root = heapArray[0];
            heapArray[0] = heapArray[--CurrentSize];
            CascadeDown(0);

            CoordinatesHashTable.Remove(root.Value.Coordinates);

            return root;
        }

        public void CascadeDown(int index) {
            int smallerChild;
            HeapNode top = heapArray[index];
            while (index < CurrentSize / 2) {
                int leftChild = 2 * index + 1;
                int rightChild = leftChild + 1;
                if (rightChild < CurrentSize && heapArray[leftChild].Key > heapArray[rightChild].Key)
                    smallerChild = rightChild;
                else
                    smallerChild = leftChild;
                if (top.Key <= heapArray[smallerChild].Key)
                    break;
                heapArray[index] = heapArray[smallerChild];
                index = smallerChild;
            }
            heapArray[index] = top;
        }
        public bool HeapIncreaseDecreaseKey(int index, float newKey) {
            if (index < 0 || index >= CurrentSize)
                return false;
            float oldKey = heapArray[index].Key;
            heapArray[index].Key = newKey;

            if (oldKey > newKey)
                CascadeUp(index);
            else
                CascadeDown(index);
            return true;
        }

        public void DisplayHeap() {
            //Console.WriteLine();
            //Console.Write("Elements of the Heap Array are : ");
            //for (int m = 0; m < CurrentSize; m++)
            //    if (heapArray[m] != null)
            //        Console.Write(heapArray[m].Key + " ");
            //    else
            //        Console.Write("-- ");
            //Console.WriteLine();
            //int emptyLeaf = 32;
            //int itemsPerRow = 1;
            //int column = 0;
            //int j = 0;
            //String separator = "...............................";
            //Console.WriteLine(separator + separator);
            //while (CurrentSize > 0) {
            //    if (column == 0)
            //        for (int k = 0; k < emptyLeaf; k++)
            //            Console.Write(' ');
            //    Console.Write(heapArray[j].Key);

            //    if (++j == CurrentSize)
            //        break;
            //    if (++column == itemsPerRow) {
            //        emptyLeaf /= 2;
            //        itemsPerRow *= 2;
            //        column = 0;
            //        Console.WriteLine();
            //    }
            //    else
            //        for (int k = 0; k < emptyLeaf * 2 - 2; k++)
            //            Console.Write(' ');
            //}
            //Console.WriteLine("\n" + separator + separator);
        }

        public PathNode GetRootValue() {
            return heapArray[0].Value;
        }

        public bool Contains(Vector2RowColumn coordinates) {
            return CoordinatesHashTable.Contains(coordinates);
        }

        public PathNode GetNode(Vector2RowColumn coordinates) {
            return (PathNode)CoordinatesHashTable[coordinates];
        }

        public void ReplaceParentNode(PathNode oldNode, PathNode newNode) {
            ((PathNode)CoordinatesHashTable[oldNode.Coordinates]).ParentNode = newNode.ParentNode;
        }
    }
}

