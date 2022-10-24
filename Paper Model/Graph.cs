using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private int test;
        /// <summary>
        /// Note that this graph still needs to be initialized.
        /// </summary>
        /// <param name="nodes"></param>
        public Graph(Node[] nodes)
        {
            this.nodes = nodes;
            Length = nodes.Length;
            test = 0;
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
        public Graph(Graph oldGraph, float scale)
        {
            Length = oldGraph.Length;
            distances = new float[Length, Length];
            for (int x = 0; x < Length; x++)
                for (int y = 0; y < Length; y++)
                    distances[x, y] = oldGraph.distances[x, y] * scale;
            nodes = new Node[Length];
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = new Node(oldGraph.nodes[i], scale);
        }
        public Graph(Graph graph1, Graph graph2, int[] start, int[] end, float[] lengths)
        {
            Length = graph1.Length + graph2.Length;
            List<Node> list = new List<Node>();
            list.AddRange(graph1.nodes);
            list.AddRange(graph2.nodes);

            nodes = list.ToArray();
            for (int i = 0; i < Length; i++)
                nodes[i].index = i;
            distances = new float[Length, Length];
            for (int x = 0; x < Length; x++)
                for (int y = 0; y < Length; y++)
                    distances[x, y] = float.MaxValue;

            for (int x = 0; x < graph1.Length; x++)
                for (int y = 0; y < graph1.Length; y++)
                    distances[x, y] = graph1.d(x, y);
            for (int x = 0; x < graph2.Length; x++)
                for (int y = 0; y < graph2.Length; y++)
                    distances[x + graph1.Length, y + graph1.Length] = graph2.d(x, y);
            for (int i = 0; i < start.Length; i++)
            {
                Node.addNeighbors(nodes[start[i]], nodes[end[i]], lengths[i]);
            }
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
        //TODO: still need to fuck the engeratrion of the effort graphs
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
                distance[i] = distances[start,i];
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
                path.Add(previousNode(start,nextNode));
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
        private Node previousNode(int start, Node node)
        {
            /*
            float distance = distances[start, node.index];
            
            for(int i =0;i<Length;i++)
            {
                if (distances[node.index, i] == float.MaxValue)
                {
                    int neighborIndex = nodes[i].neighbors.IndexOf(node);
                    if (neighborIndex != -1)
                    {
                        float neighbor2This = distances[start, i] + nodes[i].distance2Neighbor[neighborIndex];
                        if (neighbor2This == distance)
                            return nodes[i];
                    }
                }
            }
            */
            float distance = distances[start, node.index];
            for (int i = 0; i < node.reverseNeighbor.Count; i++)
            {
                Node neighbor = node.reverseNeighbor[i];
                int neighborIndex = neighbor.index;
                float neighbor2This = distances[start, neighborIndex] + 
                    neighbor.distance2Neighbor[neighbor.reverseNeighborIndex[i]];
                if (neighbor2This == distance)
                    return neighbor;
            }
            /*
            for (int i = 0; i < neighbors.Count; i++)
            {
                int neighbor = neighbors[i].index;
                int neighborNeighborIndex = neighbors[i].neighbors.IndexOf(this);
                float neighbor2This = distances[start, neighbor] + neighbors[i].distance2Neighbor[neighborNeighborIndex];
                if (neighbor2This == distance)
                    return neighbors[i];
            }
            */

            //If we get here something went wrong.
            throw new Exception();
        }
        public (List<float>,List<Node>) GetPath(Node start, Node end)
        {
            return GetPath(start.index, end.index);
        }
    }
}
