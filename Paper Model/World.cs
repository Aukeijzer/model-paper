using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paper_Model
{
    class World
    {
        private int size;
        private WorldNode[] nodes;
        private WorldNode[] bikeParkingNode;
        private WorldNode[] carParkNode;
        //The weight a node has in regards to getting people to move to that node
        private int[] pull;
        //The chance a person has to move away from that node.
        double[] push;
        Random random = new Random();

        private Graph walking;
        private Graph cycling;
        private Graph driving;
        //TODO the generation of a world isnt 
        public World(Graph distances)
        {
            //TODO: add worldnodes
            //TODO: add factors
            walking = distances.ScaleGraph(1);
            cycling = distances.ScaleGraph(0.2f);
            driving = distances.ScaleGraph(0.1f);
        }
        //havent really tested yet
        public List<Log> Tick()
        {
            updatePushPull();
            return movePeople();
        }
        private void updatePushPull()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                pull[i] = random.Next(10);
                push[i] = random.NextDouble();
            }
        }
        public struct Log
        {
            public List<Node> path;
            public float effort;

            public Log(List<Node> path, float effort)
            {
                this.path = path;
                this.effort = effort;
            }
        }
        private List<Log> movePeople()
        {
            return movePeople(1);
        }
        private List<Log> movePeople(int factor)
        {
            List<Log> logs = new List<Log>();
            int maxPull = pull.Sum();

            for(int i = 0; i < push.Length; i++)
            {
                WorldNode node = nodes[i];
                //for every person in the node roll if they will move to a different one.
                for (int j = 0; j < node.people.Count; j++)
                    if (random.NextDouble() < push[i])
                    {
                        int destination = highestCulmative(pull, random.Next(maxPull));
                        logs.Add(movePerson(i, destination, node.people[j]));
                    }
            }


            return logs;
        }
        /// <summary>
        /// TODO: create graph 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        private Log movePerson(int origin, int destination, Person person)
        {
            List<int> bikePoints = person.getBikes();
            List<int> carPoints = person.getCars();
            //Creating new graph
            List<int> start = new List<int>();
            List<int> end = new List<int>();
            List<float> lengths = new List<float>();
            //walking from start to destination
            start.Add(origin);
            end.Add(destination);
            lengths.Add(walking.d(origin, destination));
            for(int i = 0; i<carPoints.Count;i++)
            {
                //walking to a car
                start.Add(origin);
                end.Add(carPoints[i]);
                lengths.Add(walking.d(origin, carPoints[i]));
                //driving to carpark
                for(int j = 0; j < bikeParkingNode.Length; j++)
                {
                    int endIndex = bikeParkingNode[j].index;
                    start.Add(carPoints[i]);
                    end.Add(endIndex);
                    lengths.Add(driving.d(carPoints[i], endIndex));
                }
            }
            for (int i = 0; i < bikePoints.Count; i++)
            {
                //walking to a bike
                start.Add(origin);
                end.Add(bikePoints[i]);
                lengths.Add(walking.d(origin, bikePoints[i]));
                //cycling to bikePark
                for (int j = 0; j < nodes.Length; j++)
                {
                    start.Add(bikePoints[i]);
                    end.Add(j);
                    lengths.Add(driving.d(bikePoints[i], j));
                }
            }

            for(int i =0; i < bikeParkingNode.Length;i++)
            {
                int bikeParkIndex = bikeParkingNode[i].index;
                //walking from bikepark to destination
                start.Add(bikeParkIndex);
                end.Add(destination);
                lengths.Add(walking.d(bikeParkIndex, destination));
                //walking from bikepark to carpark
                for(int j = 0; j<carParkNode.Length; j++)
                {
                    int carParkIndex = carParkNode[i].index;
                    start.Add(bikeParkIndex);
                    end.Add(carParkIndex);
                    lengths.Add(walking.d(bikeParkIndex, destination));
                }
            }

            for (int i = 0; i < carParkNode.Length; i++)
            {
                //walking from carpark to destination
                int carParkIndex = bikeParkingNode[i].index;
                start.Add(carParkIndex);
                end.Add(destination);
                lengths.Add(walking.d(carParkIndex, destination));
            }

            Node[] nodeArray = Node.createNodeArray(size, start.ToArray(), end.ToArray(), lengths.ToArray());
            Graph effortGraph = new Graph(nodeArray);
            effortGraph.LazyMinSpanTree(origin, destination);

            float effort = effortGraph.d(origin, destination);
            List<Node> path = effortGraph.GetPath(origin, destination);
            Log log = new Log(path, effort);

            nodes[origin].people.Remove(person);
            nodes[destination].people.Add(person);
            return log;
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
            while (!found)
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
