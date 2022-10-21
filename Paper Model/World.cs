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
            List<Node> path;
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
            //add walking distance.
            start.Add(origin);
            end.Add(destination);
            lengths.Add(walking.d(origin, destination));
            for(int i = 0; i<carPoints.Count;i++)
            {
                //walking to car
                start.Add(origin);
                start.Add(carPoints[i]);
                lengths.Add(walking.d(origin, carPoints[i]));

                //TODO: add edges between car and all carpoints
                // also need to add the walking distance from all carpoints to all other carpoints and bikepoints, etc.
                // havent really figured out what needs to be added yet to get the minimum effort.
            }
            Node[] nodeArray = Node.createNodeArray(size, start.ToArray(), end.ToArray(), lengths.ToArray());
            Graph effortGraph = new Graph(nodeArray);
            nodes[origin].people.Remove(person);
            nodes[destination].people.Add(person);
            return default;
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
