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

        private Graph distances;
        private Graph walking;
        private Graph cycling;
        private Graph driving;

        public World(WorldNode[] nodes)
        {
            this.nodes = nodes;
            distances = new Graph(nodes);
            distances.Initialize();
            //TODO: add factors
            walking = distances.ScaleGraph(1);
            cycling = distances.ScaleGraph(0.2f);
            driving = distances.ScaleGraph(0.1f);
            bikeParkingCost = 5;
            carParkingCost = 7.5f;
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
        //still buggy
        public List<Log> Tick()
        {
            updatePushPull();
            List<Log> logs = movePeople();
            for (int i = 0; i < logs.Count; i++)
                executeLog(logs[i]);
            return logs;
        }
        private float effortCost<T>(int start, int end) where T : Vehicle, new()
        {
            return effortCost(start, end, new T());
        }
        private float effortCost(int start, int end, Vehicle vehicle)
        {
            if (vehicle is Car)
                return driving.d(start, end) + 2 * carParkingCost;
            else if (vehicle is Bike)
                return cycling.d(start, end) + 2 * bikeParkingCost;
            else
                return walking.d(start, end);
        }
        private void executeLog(Log log)
        {
            List<Node> path = log.path;
            log.person.move(path[path.Count-1], nodes);
            List<Vehicle> vehicles = log.vehicles;
            for (int i = 0; i < vehicles.Count; i++)
                if (!(vehicles[i] is Legs))
                    vehicles[i].move(path[i + 1], nodes);
        }
        private void updatePushPull()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                pull[i] = random.Next(5);
                push[i] = random.NextDouble();
            }
        }
        public struct Log
        {
            public Person person;
            public List<Node> path;
            public float totalEffort;
            public List<Vehicle> vehicles;
            public List<float> efforts;

            public Log(List<Node> path, float totalEffort, List<Vehicle> vehicles, Person person, List<float> efforts)
            {
                this.path = path;
                this.totalEffort = totalEffort;
                this.vehicles = vehicles;
                this.person = person;
                this.efforts = efforts;
            }
            public override string ToString()
            {
                string printable = "";
                printable += Node.PrintList(path, vehicles);
                printable += "\n" + "Effort:" + totalEffort;
                return printable;
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
            List<Vehicle> vehicles = person.vehicles;
            //Creating new graph
            List<int> start = new List<int>();
            List<int> end = new List<int>();
            List<float> lengths = new List<float>();
            //walking from start to destination
            start.Add(origin);
            end.Add(destination);
            lengths.Add(effortCost<Legs>(origin, destination));
            for(int i = 0; i < vehicles.Count; i++)
                if(!vehicles[i].moving) 
                {
                    Vehicle vehicle = vehicles[i];
                    //walking to a vehicle
                    start.Add(origin);
                    end.Add(vehicle.location);
                    lengths.Add(effortCost<Legs>(origin, vehicle.location));
                    //driving to carpark
                    if(vehicle is Car)
                        for (int j = 0; j < bikeParkingNodes.Count; j++)
                        {
                            int endIndex = bikeParkingNodes[j].index;
                            start.Add(vehicle.location);
                            end.Add(endIndex);
                            lengths.Add(effortCost(vehicle.location, endIndex,vehicle));
                        }
                    //cycling to bikePark
                    if (vehicle is Bike)
                        for (int j = 0; j < carParkNodes.Count; j++)
                        {
                            int endIndex = carParkNodes[j].index;
                            start.Add(vehicle.location);
                            end.Add(endIndex);
                            lengths.Add(effortCost(vehicle.location, endIndex,vehicle));
                        }
                }

            for (int i = 0; i < bikeParkingNodes.Count; i++)
            {
                int bikeParkIndex = bikeParkingNodes[i].index;
                //walking from bikepark to destination
                start.Add(bikeParkIndex);
                end.Add(destination);
                lengths.Add(effortCost<Legs>(bikeParkIndex, destination));
                //walking from bikepark to carpark
                for (int j = 0; j < carParkNodes.Count; j++)
                    {
                        int carParkIndex = carParkNodes[j].index;
                        start.Add(bikeParkIndex);
                        end.Add(carParkIndex);
                        lengths.Add(effortCost<Legs>(bikeParkIndex, destination));
                    }
            }

            for (int i = 0; i < carParkNodes.Count; i++)
            {
                //walking from carpark to destination
                int carParkIndex = carParkNodes[i].index;
                start.Add(carParkIndex);
                end.Add(destination);
                lengths.Add(effortCost<Legs>(carParkIndex, destination));
            }
            Node[] nodeArray = Node.createNodeArray(size, start.ToArray(), end.ToArray(), lengths.ToArray());
            return new Graph(nodeArray);
        }
        private Vehicle determineTravelType(int start, int end, float effort, Person person)
        {
            if (effort == effortCost<Car>(start, end))
            {
                List<Car> cars = person.getSpecificVehicle<Car>();
                for (int j = 0; j<cars.Count; j++)
                    if (!cars[j].moving && cars[j].location == start)
                        return cars[j];
            }
            else if (effort == effortCost<Bike>(start, end))
            {
                List<Bike> bikes = person.getSpecificVehicle<Bike>();
                for (int j = 0; j<bikes.Count; j++)
                    if (!bikes[j].moving && bikes[j].location == start)
                        return bikes[j];
            }
            
            return new Legs();
        }
        private Log movePerson(int origin, int destination, Person person)
        {
            Graph effortGraph = createEffortGraph(origin, destination, person);
            effortGraph.LazyMinSpanTree(origin, destination);
            float totalEffort = effortGraph.d(origin, destination);
            (List<float> efforts,List<Node> path) = effortGraph.GetPath(origin, destination);
            List<Vehicle> vehicles = new List<Vehicle>();
            for(int i = 0; i<path.Count-1 ; i++)
            {
                WorldNode start = nodes[path[i].index];
                WorldNode end = nodes[path[i + 1].index];
                float effort = efforts[i+1] - efforts[i];
                Vehicle vehicle = determineTravelType(start.index, end.index, effort, person);
                vehicle.moving = true;
                vehicles.Add(vehicle);
            }
            person.moving = true;

            Log log = new Log(path, totalEffort,vehicles,person, efforts);
            int x;
            if (log.path.Count > 2)
                x = 6;
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
