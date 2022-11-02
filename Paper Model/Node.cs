using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paper_Model
{
    public class Node
    {
        public List<Node> neighbors = new List<Node>();
        public List<float> distance2Neighbor = new List<float>();
        public List<Node> reverseNeighbor = new List<Node>();
        public List<float> reverseNeighborDistance = new List<float>();
        public int index;
        public override string ToString()
        {
            return index.ToString();
        }
        public static string PrintList(List<Node> nodes, List<Vehicle> vehicles)
        {
            string s = "";
            for (int i = 1; i < nodes.Count; i++)
            {
                s += "From: " + nodes[i - 1] + " ";
                s += "To: " + nodes[i] + " ";
                if (vehicles.Count >= i)
                {
                    s += "Mode: " + vehicles[i - 1] + "     ";
                } //Je kunt het!
                else
                {
                    s += "Mode: Walking";
                }
            }
            return s;
        }
        public Node(int index)
        {
            this.index = index;
        }
        public Node(Node node, float scale)
        {
            index = node.index;
            neighbors = new List<Node>(node.neighbors.Count);
            distance2Neighbor = new List<float>();
            for (int i = 0; i < node.distance2Neighbor.Count; i++)
            {
                distance2Neighbor.Add(node.distance2Neighbor[i] * scale);
                neighbors.Add(node.neighbors[i]);
            }
            reverseNeighbor = node.reverseNeighbor;
            reverseNeighborDistance = node.reverseNeighborDistance;
        }

        public void updatedistances(MinHeap heap, float distance)
        {
            for (int i = 0; i < neighbors.Count; i++)
            {
                float newDistance = distance + distance2Neighbor[i];
                heap.Update(newDistance, neighbors[i]);
            }
        }
        public static void updateNeighbor(Node start, Node end, float distance)
        {
            int startIndex = start.neighbors.IndexOf(end);
            if (distance < start.distance2Neighbor[startIndex])
            {
                start.distance2Neighbor[startIndex] = distance;
                int endIndex = end.reverseNeighbor.IndexOf(start);
                end.reverseNeighborDistance[endIndex] = distance;
            }
        }
        public static void updateNeighbors(Node nodeA, Node nodeB, float distance)
        {
            updateNeighbor(nodeA, nodeB, distance);
            updateNeighbor(nodeB, nodeA, distance);
        }
        public static void addNeighbors(Node nodeA, Node nodeB, float distance)
        {
            if (nodeA.index != nodeB.index && distance > 0)
            {
                nodeA.addNeighbor(nodeB, distance);
                nodeB.addNeighbor(nodeA, distance);
            }
        }
        public void addNeighbor(Node node, float distance)
        {
            neighbors.Add(node);
            distance2Neighbor.Add(distance);
            node.reverseNeighbor.Add(this);
            node.reverseNeighborDistance.Add(distance);

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

            bool[,] edge = new bool[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    edge[i, j] = false;
            Node[] nodeArray = new Node[size];
            for (int i = 0; i < size; i++)
                nodeArray[i] = new Node(i);
            for (int i = 0; i < start.Length; i++)
            {
                if (distance[i] > 0 && start[i] != end[i])
                {
                    if (edge[start[i], end[i]] || edge[end[i],start[i]])
                        Node.updateNeighbors(
                            nodeArray[start[i]],
                            nodeArray[end[i]],
                            distance[i]);

                    else
                        Node.addNeighbors(
                            nodeArray[start[i]],
                            nodeArray[end[i]],
                            distance[i]);
                    edge[start[i], end[i]] = true;
                }

            }

            return nodeArray;
        }
        public static Node[] createDirectedNodeArray(int size, int[] start, int[] end, float[] distance)
        {
            bool[,] edge = new bool[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    edge[i, j] = false;
            Node[] nodeArray = new Node[size];
            for (int i = 0; i < size; i++)
                nodeArray[i] = new Node(i);
            for (int i = 0; i < start.Length; i++)
                if (distance[i] > 0 && start[i] != end[i])
                {
                    if (!edge[start[i], end[i]])
                        nodeArray[start[i]].addNeighbor(nodeArray[end[i]], distance[i]);
                    else
                    {
                        Node node = nodeArray[start[i]];
                        Node endNode = nodeArray[end[i]];
                        updateNeighbor(node, endNode, distance[i]);
                    }
                    edge[start[i], end[i]] = true;
                }

                

            return nodeArray;
        }
        public static Node[] createNodeGrid(int width, int height, float distance)
        {
            int size = width * height;
            Random random = new Random();
            
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
                    lengths.Add((float)random.NextDouble()*distance+1);
                    //lengths.Add(distance);
                    start.Add(thisNode);
                    end.Add(thisNode + width);
                    lengths.Add((float)random.NextDouble()*distance+1);
                    //lengths.Add(distance);
                }
            //adding edges to bottom row
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

            return Node.createNodeArray(size, start.ToArray(), end.ToArray(), lengths.ToArray());
        }
        public Node getPrevious(int start, float[,] distances)
        {
            float distance = distances[start, index];
            for (int i = 0; i<neighbors.Count; i++)
            {
                int neighbor = neighbors[i].index;
                float neighbor2This = distances[start, neighbor] + distance2Neighbor[i];
                if (neighbor2This == distance)
                    return neighbors[i];
            }
            for (int i = 0; i < neighbors.Count; i++)
            {
                int neighbor = neighbors[i].index;
                int neighborNeighborIndex = neighbors[i].neighbors.IndexOf(this);
                float neighbor2This = distances[start, neighbor] + neighbors[i].distance2Neighbor[neighborNeighborIndex];
                if (neighbor2This == distance)
                    return neighbors[i];
            }

            //If we get here something went wrong.
            throw new Exception();
        }
    }
}
