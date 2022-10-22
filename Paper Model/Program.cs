using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paper_Model
{
    class Program
    {
        static void Main(string[] args)
        {
            //    int[] edges =
            //    {
            //        0,1,1,
            //        0,2,2,
            //        1,3,5,
            //        2,3,3,
            //    };
            //    Node[] nodes = Node.createNodeArray(4, edges);
            //    Graph graph = new Graph(nodes);
            //    Console.WriteLine(graph.d(nodes[0], nodes[3]).ToString());
            //    MinHeap minHeap = new MinHeap();
            //    var keys = new List<int> { 6, 5, 4, 3, 2, 1, 0 };
            //    var values = new List<int> { 60, 16, 19, 17, 8, 5, 3 };
            //    Console.ReadLine();
            //  graph2.Initialize();
            //  Console.WriteLine(graph2.d(5, 55).ToString());
            //  List<Node> path = graph2.GetPath(5, 55);
            //  Console.WriteLine(Node.PrintList(path));
            Node[] nodes = Node.createNodeGrid(10, 10, 10);
            Graph graph2 = new Graph(nodes);
            WorldNode[] worldNodes = new WorldNode[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                worldNodes[i] = new WorldNode(nodes[i]);
            worldNodes[0].addFamily(5, 1, 10);
            World world = new World(worldNodes);
            for(int i = 0; i < 10; i++)
            {
                var logs = world.Tick();
                for (int j = 0; j < logs.Count; j++)
                    Console.WriteLine(logs[j].ToString());
                Console.WriteLine("---");
            }
            Console.ReadLine();
        }
    }
}
