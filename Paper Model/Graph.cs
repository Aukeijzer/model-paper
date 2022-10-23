using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paper_Model
{
    class Graph
    {
        public int Length;
        Node[] nodes;
        float[,] distances;
        /// <summary>
        /// Note that this graph still needs to be initialized.
        /// </summary>
        /// <param name="nodes"></param>
        public Graph(Node[] nodes)
        {
            this.nodes = nodes;
            Length = nodes.Length;
            //set the distance between all nodes as infinity
            distances = new float[Length, Length];
            for (int x = 0; x < Length; x++)
                for (int y = 0; y < Length; y++)
                    distances[x, y] = float.MaxValue;
        }
        public Graph(int length, Node[] nodes, float[,] distances)
        {
            Length = length;
            this.nodes = nodes;
            this.distances = distances;
        }
        public Graph ScaleGraph(float scale)
        {
            float[,] newDistances = new float[Length, Length];
            for (int x = 0; x < Length; x++)
                for (int y = 0; y < Length; y++)
                    newDistances[x, y] = distances[x, y] * scale;
            return new Graph(Length, nodes, newDistances);
        }
        public void Initialize()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                float[] tree = minSpanTree(nodes[i]);
                for (int j = 0; j < nodes.Length; j++)
                    distances[i, j] = tree[j];
            }
        }
        public float d(int start, int end)
        {
            return distances[start, end];
        }
        public float d(Node start, Node end)
        {
            return d(start.index, end.index);
        }
        private static MinHeap createMinHeap(Node node, Node[] nodes)
        {
            MinHeap heap = new MinHeap();
            for (int i = 0; i < nodes.Length; i++)
                heap.Insert(float.MaxValue, nodes[i]);
            heap.Update(0, node);
            return heap;
        }
        private float[] minSpanTree(Node node)
        {
            float[] distance = new float[nodes.Length];
            for (int i = 0; i < distance.Length; i++)
                distance[i] = float.MaxValue;
            MinHeap heap = createMinHeap(node,nodes);
            bool finished = false;
            while (!finished)
            {
                (float minD, Node minNode) = heap.Pop();
                //Can't reach any more nodes.
                if (minD == float.MaxValue)
                    finished = true;
                else
                {
                    distance[minNode.index] = minD;
                    minNode.updatedistances(heap, minD);
                }

            }
            return distance;
        }
        /// <summary>
        /// Lazy version of MinSpanTree
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public void LazyMinSpanTree(int start, int end)
        {
            //This code is shamelessly copy pasted and could possibly be improved.
            float[] distance = new float[nodes.Length];
            for (int i = 0; i < distance.Length; i++)
                distance[i] = float.MaxValue;
            MinHeap heap = createMinHeap(nodes[start],nodes);
            bool finished = false;
            while (!finished)
            {
                (float minD, Node minNode) = heap.Pop();
                //Can't reach any more nodes.
                if (minNode.index == end)
                    finished = true;
                distance[minNode.index] = minD;
                minNode.updatedistances(heap, minD);

            }
            for (int i = 0; i < Length; i++)
                distances[start, i] = distance[i];
        }
        public (List<float>,List<Node>) GetPath(int start, int end)
        {
            List<Node> path = new List<Node>();
            List<float> distance2start = new List<float>();
            path.Add(nodes[end]);
            float distance = distances[start, end];
            distance2start.Add(distance);
            Node nextNode = path[path.Count - 1];
            while (distance > 0)
            {
                path.Add(nextNode.getPrevious(start,distances));
                nextNode = path[path.Count - 1];
                distance = distances[start, nextNode.index];
                distance2start.Add(distance);
            }
            if (path.Count == 0)
            {
                path.Add(nodes[start]);
                distance2start.Add(0);
            }
            else
            {
                path.Reverse();
                distance2start.Reverse();
            }
            return (distance2start,path);
        }

        public (List<float>,List<Node>) GetPath(Node start, Node end)
        {
            return GetPath(start.index, end.index);
        }
    }
}
