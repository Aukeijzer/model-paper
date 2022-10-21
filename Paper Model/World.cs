using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paper_Model
{
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

        private Graph walking;
        private Graph cycling;
        private Graph driving;
        public World(Graph distances)
        {
            walking = distances;
            //TODO: add factors
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
                push[i] = random.Next(10);
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
            //TODO: push probably needs to be reworked.
            int[] realPush = new int[push.Length];
            for (int i = 0; i < push.Length; i++)
                realPush[i] = push[i] * nodes[i].people;
            int maxPush = realPush.Sum();
            int amount = maxPush * factor / 10;
            for (int i = 0; i < amount; i++)
            {
                int origin = highestCulmative(realPush, random.Next(maxPush));
                //Remove the push the person provided.
                realPush[origin] -= push[i];
                maxPush -= push[i];
                int destination = highestCulmative(pull, random.Next(maxPull));
                logs.Add(movePerson(origin, destination));
            }
            return logs;
        }
        private Log movePerson(int origin, int destination)
        {
            List<int> bikePoints = new List<int>();
            List<int> carPoints = new List<int>();
            for(int i = 0;i<nodes.Length;i++)
            {
                WorldNode node = nodes[i];
                if (node.bikes != 0)
                    bikePoints.Add(node.index);
                if (node.cars != 0)
                    carPoints.Add(node.index);
            }
            nodes[origin].people--;
            nodes[destination].people++;
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
