using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paper_Model
{
    class Graph
    {
        public int length;
        Node[] nodes;
        float[,] distances;

        public Graph(Node[] nodes)
        {
            this.nodes = nodes;
            length = nodes.Length;
            Initialize();
        }
        public Graph(int length, Node[] nodes, float[,] distances)
        {
            this.length = length;
            this.nodes = nodes;
            this.distances = distances;
        }
        public Graph ScaleGraph(float scale)
        {
            float[,] newDistances = new float[length, length];
            for (int x = 0; x < length; x++)
                for (int y = 0; y < length; y++)
                    newDistances[x, y] = distances[x, y] * scale;
            return new Graph(length, nodes, distances);
        }
        /// <summary>
        /// Creates a graph of a grid.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="distance"></param>
        public Graph(int width, int height, float distance)
        {
            length = width * height;
            List<int> start = new List<int>();
            List<int> end = new List<int>();
            List<float> lengths = new List<float>();
            //adding edges between all top left nodes
            for (int x = 0; x < width - 1; x++)
                for (int y = 0; y < height - 1; y++)
                {
                    int thisNode = y * width + x;
                    start.Add(thisNode);
                    end.Add(thisNode + 1);
                    lengths.Add(distance);

                    start.Add(thisNode);
                    end.Add(thisNode + width);
                    lengths.Add(distance);
                }
            //adding eddges to bottom row
            for (int x = 0; x < width - 1; x++)
            {
                int thisNode = width * (height - 1) + x;
                start.Add(thisNode);
                end.Add(thisNode + 1);
                lengths.Add(distance);
            }
            //adding edges to right column
            for (int y = 0; y < height - 1; y++)
            {
                int thisNode = y * height + (width - 1);
                start.Add(thisNode);
                end.Add(thisNode + width);
                lengths.Add(distance);
            }
            nodes = Node.createNodeArray(length, start.ToArray(),end.ToArray(),lengths.ToArray());
            Initialize();
        }

        private void Initialize()
        {
            //set the distance between all nodes as infinity
            distances = new float[nodes.Length, nodes.Length];

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
        private MinHeap createMinHeap(Node node)
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
            MinHeap heap = createMinHeap(node);
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
        
        public List<Node> getPath(int start, int end)
        {
            List<Node> path = new List<Node>();
            path.Add(nodes[end]);
            float distance = distances[start, end];
            Node nextNode = path[path.Count - 1];
            while (distance > 0)
            {
                path.Add(nextNode.getPrevious(start,distances));
                nextNode = path[path.Count - 1];
                distance = distances[start, nextNode.index];
            }
            if (path.Count == 0)
                path.Add(nodes[start]);
            else
                path.Reverse();
            return path;
        }

        public List<Node> getPath(Node start, Node end)
        {
            return getPath(start.index, end.index);
        }
    }
}
