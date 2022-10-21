using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Paper_Model
{
    /// <summary>
    /// Used to efficiently get the lowest element in a list
    /// </summary>
    class MinHeap
    {
        private List<float> distances;
        private List<Node> nodes;
        private int size;

        //Keeps track of where all values were stored.
        private List<int> keys;

        public MinHeap()
        {
            distances = new List<float> { float.MaxValue };
            nodes = new List<Node> { null };
            size = 0;
            keys = new List<int>();
        }
        public MinHeap(List<float> distances, List<Node> nodes) : this()
        {
            this.distances.AddRange(distances);
            this.nodes.AddRange(nodes);
            size = distances.Count;
            for (int i = 0; i < size; i++)
                keys.Add(i);
        }
        /// <summary>
        /// Returns the lowest value.
        /// </summary>
        public (float,Node) Peek()
        {
            return (distances[1], nodes[1]);
        }
        /// <summary>
        /// Removes and returns the lowest value.
        /// </summary>
        /// <returns></returns>
        public (float, Node) Pop()
        {
            //heap is empty
            if (size == 0)
                return (int.MaxValue, null);

            var tuple = Peek();

            //removing first value and putting the last value as first
            swap(1, size);
            distances.RemoveAt(size);
            nodes.RemoveAt(size);

            size--;
            heapifyDown(1);

            return tuple;
        }
        /// <summary>
        /// Insert a value in the heap
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="node"></param>
        public void Insert(float distance, Node node)
        {
            distances.Add(distance);
            nodes.Add(node);
            size++;
            keys.Add(size);
            heapifyUp(size);
        }
        /// <summary>
        /// If a value is lower, update it to given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        public void Update(float value, Node node)
        {
            int key = keys[node.index];
            if ( key <= size && distances[key] > value)
            {
                distances[key] = value;
                heapifyUp(key);
            }
        }
        private void heapifyDown(int index)
        {
            int leftNode = index * 2;
            int rightNode = index * 2 + 1;

            //Node is last node in tree
            if (size < leftNode) return;

            if (distances[index] > distances[leftNode])
            {
                swap(leftNode, index);
                heapifyDown(leftNode);
                return;
            }

            if (size < rightNode) return;

            if (distances[index] > distances[rightNode])
            {
                swap(rightNode, index);
                heapifyDown(rightNode);
                return;
            }
        }
        private void heapifyUp(int index)
        {
            int parentNode = index / 2;
            //node is rootnode
            if (parentNode < 1) return;
            
            if (distances[index]<distances[parentNode])
            {
                swap(index, parentNode);
                heapifyUp(parentNode);
            }
        }
        private void swap(int index1, int index2)
        {
            int key1 = nodes[index1].index;
            int key2 = nodes[index2].index;
            keys[key1] = index2;
            keys[key2] = index1;
            swapList(ref distances, index1, index2);
            swapList(ref nodes, index1, index2);
        }
        private static void swapList<T>(ref List<T> list, int index1, int index2)
        {
            T value1 = list[index1];
            T value2 = list[index2];
            list[index1] = value2;
            list[index2] = value1;
        }
    }
}