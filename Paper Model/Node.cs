using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paper_Model
{
    class Node
    {
        public List<Node> neighbors = new List<Node>();
        protected List<float> distance2Neighbor = new List<float>();
        public int index;
        public Node(int index)
        {
            this.index = index;
        }

        public void updatedistances(MinHeap heap, float distance)
        {
            for (int i = 0; i < neighbors.Count; i++)
            {
                float newDistance = distance + distance2Neighbor[i];
                heap.Update(newDistance, neighbors[i]);
            }
        }
        public static void addNeighbors(Node nodeA, Node nodeB, float distance)
        {
            nodeA.addNeighbor(nodeB, distance);
            nodeB.addNeighbor(nodeA, distance);
        }
        protected void addNeighbor(Node node, float distance)
        {
            neighbors.Add(node);
            distance2Neighbor.Add(distance);
        }
        /// <summary>
        /// outdated function
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
                Node.addNeighbors(
                    nodeArray[edges[i]],
                    nodeArray[edges[i + 1]],
                    edges[i + 2]);
            return nodeArray;
        }
        /// <summary>
        /// creates and initializes an array of nodes.
        /// </summary>
        /// <param name="size">the amount of nodes</param>
        /// <param name="start">startpoint of an edge</param>
        /// <param name="end">end point of an edge</param>
        /// <param name="distance">lenght of edge</param>
        /// <returns></returns>
        public static Node[] createNodeArray(int size, int[] start, int[] end, float[] distance)
        {
            Node[] nodeArray = new Node[size];
            for (int i = 0; i < size; i++)
                nodeArray[i] = new Node(i);
            for (int i = 0; i < start.Length; i++)
                Node.addNeighbors(
                    nodeArray[start[i]],
                    nodeArray[end[i]],
                    distance[i]);
            return nodeArray;
        }
    }
    class WorldNode : Node
    {
        public WorldNode(int index) : base(index) 
        {
        }
        public int people;
        public int bikes;
        public int cars;

    }
}
