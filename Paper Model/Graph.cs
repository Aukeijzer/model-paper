using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paper_Model
{
    class Graph
    {
        Node[] nodes;
        int[,] distances;

        public Graph(Node[] nodes)
        {
            this.nodes = nodes;
            Initialize();
        }
        private void Initialize()
        {
            //set the distance between all nodes as infinity
            distances = new int[nodes.Length, nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                for (int j = 0; j < nodes.Length; j++)
                    distances[i, j] = int.MaxValue;
            //calculate the minimum spanning tree for all nodes
            for(int i = 0; i < nodes.Length; i++)
            {
                int[] tree = minSpanTree(nodes[i]);
                for (int j = 0; j < nodes.Length; j++)
                    distances[i, j] = tree[j];
            }
        }

        public int d(Node start, Node end)
        {
            return distances[start.index, end.index];
        }
        private int[] minSpanTree(Node node)
        {
            int[] distance = new int[nodes.Length];
            for (int i = 0; i < distance.Length; i++)
                distance[i] = int.MaxValue;
            MinHeap heap = new MinHeap();
            for (int i = 0; i < nodes.Length; i++)
                heap.Insert(int.MaxValue, nodes[i]);
            heap.Update(0, node);
            bool finished = false;
            while (!finished)
            {
                (int minD, Node minNode) = heap.Pop();
                //Can't reach any more nodes.
                if (minD == int.MaxValue)
                    finished = true;
                else
                {
                    distance[minNode.index] = minD;
                    minNode.updatedistances(heap, minD);
                }

            }
            return distance;
        }
    }
    class Node
    {
        public List<Node> neighbors = new List<Node>();
        public List<int> distance2Neighbor = new List<int>();
        public int index;
        public Node(int index)
        {
            this.index = index;
        }

        public void updatedistances(MinHeap heap, int distance)
        {
            for(int i = 0; i < neighbors.Count; i++)
            {
                int newDistance = distance + distance2Neighbor[i];
                heap.Update(newDistance, neighbors[i]);
            }
        }
        public static void addNeighbors(Node nodeA, Node nodeB, int distance)
        {
            nodeA.addNeighbor(nodeB, distance);
            nodeB.addNeighbor(nodeA, distance);
        }
        private void addNeighbor(Node node,int distance)
        {
            neighbors.Add(node);
            distance2Neighbor.Add(distance);
        }
        /// <summary>
        /// creates and initializes an array of nodes 
        /// </summary>
        /// <param name="size">the amount of nodes</param>
        /// <param name="edges">the edges between the nodes</param>
        /// <returns></returns>
        public static Node[] createNodeArray(int size, int[] edges)
        {
            Node[] nodeArray = new Node[size];
            for (int i = 0; i < nodeArray.Length; i++)
                nodeArray[i] = new Node(i);
            for (int i = 0; i < edges.Length; i += 3)
                Node.addNeighbors(nodeArray[edges[i]], nodeArray[edges[i + 1]], edges[i + 2]);
            return nodeArray;
        }
    }
}
