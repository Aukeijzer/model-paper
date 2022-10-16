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
        protected List<int> distance2Neighbor = new List<int>();
        public int index;
        public Node(int index)
        {
            this.index = index;
        }

        public void updatedistances(MinHeap heap, int distance)
        {
            for (int i = 0; i < neighbors.Count; i++)
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
        protected void addNeighbor(Node node, int distance)
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
