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
        public int d(int start, int end)
        {
            return distances[start, end];
        }
        public int d(Node start, Node end)
        {
            return d(start.index, end.index);
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
        /// <summary>
        /// Creates a graph of a grid.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="distance"></param>
        public Graph(int width, int height, int distance)
        {
            int size = width * height;
            List<int> edges = new List<int>();
            
            //adding edges between all top left nodes
            for(int x = 0; x<width-1;x++)
                for(int y = 0; y < height-1; y++)
                {
                    int thisNode = y * width + x;
                    edges.Add(thisNode);
                    edges.Add(thisNode + 1);
                    edges.Add(distance);

                    edges.Add(thisNode);
                    edges.Add(thisNode + width);
                    edges.Add(distance);
                }
            //adding eddges to bottom row
            for(int x=0; x < width-1; x++)
            {
                int thisNode = width * (height - 1) + x;
                edges.Add(thisNode);
                edges.Add(thisNode + 1);
                edges.Add(distance);
            }
            //adding edges to right column
            for(int y=0; y < height-1;y++)
            {
                int thisNode = y * height + (width - 1);
                edges.Add(thisNode);
                edges.Add(thisNode + width);
                edges.Add(distance);
            }
            nodes = Node.createNodeArray(size, edges.ToArray());
            Initialize();
        }
        /* current idea for calculating way of moving
         * have 3 graphs: 1 for walking, 1 for cycling, 1 for driving (these are identical).
         * Make connections between the walking and cycling/driving graphs at the points where cars/bikes are available
         * dont really know how to do this efficiently
        */
        class World
        {
            private WorldNode[] nodes;
            private int[] pull;
            private int[] push;
            Random random = new Random();
            //havent really tested yet
            public void tick()
            {
                movePeople(20);
                updatePushPull();
            }
            private void updatePushPull()
            {
                for(int i = 0; i < nodes.Length; i++)
                {
                    pull[i] = random.Next(10);
                    push[i] = random.Next(10);
                }
            }
            private void movePeople(int amount)
            {
                int maxPull = pull.Sum();
                int[] realPush = new int[push.Length];
                for (int i = 0; i < push.Length; i++)
                    realPush[i] = push[i] * nodes[i].people;
                int maxPush = realPush.Sum();
                for(int i = 0; i<amount;i++)
                {
                    int origin = highestCulmative(realPush, random.Next(maxPush));
                    realPush[origin] -= push[i];
                    maxPush -= push[i];
                    int destination = highestCulmative(pull, random.Next(maxPull));
                    //TODO: MAYBE DO SOMETHING ELSE HERE
                    Console.WriteLine("Moved from " + origin.ToString() + "to " + destination.ToString());
                    nodes[origin].people--;
                    nodes[destination].people++;
                }
            }
            /// <summary>
            /// Returns the index for the highest possible culmative value in the list lower then the bound
            /// </summary>
            /// <param name="list"></param>
            /// <param name="bound"></param>
            /// <returns></returns>
            private static int highestCulmative(int[] list, int bound)
            {
                int index = int.MaxValue;
                bool found = false;
                int i = 0;
                while(!found)
                {
                    if (bound < list[i])
                    {
                        index = i - 1;
                        found = true;
                    }
                    else
                    {
                        bound -= list[i];
                        i++;
                    }
                }
                return index;
            }
        }
    }
}
