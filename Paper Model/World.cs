using System;
using System.Collections.Generic;
using System.Linq;

namespace Paper_Model
{
    class World
    {
        private int size;
        private WorldNode[] nodes;
        private List<WorldNode> bikeParkingNodes = new List<WorldNode>();
        private float bikeParkingCost;
        private List<WorldNode> carParkNodes = new List<WorldNode>();
        private float carParkingCost;
        //The weight a node has in regards to getting people to move to that node
        private int[] pull;
        //The chance a person has to move away from that node.
        double[] push;
        Random random = new Random();

        private Graph walking;
        private Graph cycling;
        private Graph driving;

        public World(WorldNode[] nodes)
        {
            this.nodes = nodes;
            Graph distances = new Graph(nodes);
            distances.Initialize();
            //TODO: add factors
            walking = distances.ScaleGraph(1);
            cycling = distances.ScaleGraph(0.2f);
            driving = distances.ScaleGraph(0.1f);
            bikeParkingCost = 1;
            carParkingCost = 2;
            size = nodes.Length;
            pull = new int[size];
            push = new double[size];
            for(int i = 0; i < size; i++)
            {
                WorldNode node = nodes[i];
                if (node.bikePark)
                    bikeParkingNodes.Add(node);
                if (node.carPark)
                    carParkNodes.Add(node);
            }

        }
        //havent really tested yet
        public List<Log> Tick()
        {
            updatePushPull();
            //TODO: the idea is that people move based on the returned logs, but i am tired. Goodnight
            //Also not fully tested
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
            public Car car;
            public Bike bike;

            public Log(List<Node> path, float effort, Car car, Bike bike)
            {
                this.path = path;
                this.effort = effort;
                this.car = car;
                this.bike = bike;
            }
            public override string ToString()
            {
                return Node.PrintList(path);
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
        private Graph createEffortGraph(int origin, int destination, Person person)
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
            for (int i = 0; i < carPoints.Count; i++)
            {
                //walking to a car
                start.Add(origin);
                end.Add(carPoints[i]);
                lengths.Add(walking.d(origin, carPoints[i]));
                //driving to carpark
                for (int j = 0; j < bikeParkingNodes.Count; j++)
                {
                    int endIndex = bikeParkingNodes[j].index;
                    start.Add(carPoints[i]);
                    end.Add(endIndex);
                    lengths.Add(driving.d(carPoints[i], endIndex)+2*carParkingCost);
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
                    lengths.Add(cycling.d(bikePoints[i], j)+2*bikeParkingCost);
                }
            }

            for (int i = 0; i < bikeParkingNodes.Count; i++)
            {
                int bikeParkIndex = bikeParkingNodes[i].index;
                //walking from bikepark to destination
                start.Add(bikeParkIndex);
                end.Add(destination);
                lengths.Add(walking.d(bikeParkIndex, destination));
                //walking from bikepark to carpark
                for (int j = 0; j < carParkNodes.Count; j++)
                {
                    int carParkIndex = carParkNodes[i].index;
                    start.Add(bikeParkIndex);
                    end.Add(carParkIndex);
                    lengths.Add(walking.d(bikeParkIndex, destination));
                }
            }

            for (int i = 0; i < carParkNodes.Count; i++)
            {
                //walking from carpark to destination
                int carParkIndex = bikeParkingNodes[i].index;
                start.Add(carParkIndex);
                end.Add(destination);
                lengths.Add(walking.d(carParkIndex, destination));
            }
            Node[] nodeArray = Node.createNodeArray(size, start.ToArray(), end.ToArray(), lengths.ToArray());
            return new Graph(nodeArray);
        }
        private Log movePerson(int origin, int destination, Person person)
        {
            Graph effortGraph = createEffortGraph(origin, destination, person);
            effortGraph.LazyMinSpanTree(origin, destination);

            float effort = effortGraph.d(origin, destination);
            List<Node> path = effortGraph.GetPath(origin, destination);
            Car car=new Car(-1);
            Bike bike=new Bike(-1);
            for(int i = 0; i<path.Count-2 ; i++)
            {
                WorldNode start = nodes[path[i].index];
                WorldNode end = nodes[path[i + 1].index];
                if (end.carPark && person.getCars().Contains(start.index))
                    car=start.useCar(person);
                else if (end.bikePark && person.getBikes().Contains(start.index))
                    bike=start.useBike(person);
            }
            Log log = new Log(path, effort, car, bike);

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
                    index = i;
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
