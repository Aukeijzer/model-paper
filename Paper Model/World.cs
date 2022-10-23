using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms.VisualStyles;

namespace Paper_Model
{
    public class World
    {
        private int size;
        private WorldNode[] nodes;
        private List<WorldNode> bikeParkingNodes = new List<WorldNode>();
        private float bikeParkingCost;
        private List<WorldNode> carParkNodes = new List<WorldNode>();
        private float carParkingCost;
        private float carEmissions;
        private float gasPrice;
        private float maxWalkingDistance;
        private float maxCyclingDistance;
        public static int Time;
        public static int carUsage;
        public static int bikeUsage;
        public static int legsUsage;
        public static int totalCarKM;

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
            driving = distances.ScaleGraph(0.03f);
            bikeParkingCost = 1;
            carParkingCost = 10f;
            gasPrice = 0.14f; // 0.14 euro per km
            carEmissions = 0.1f; // 100g CO2 per km
            maxWalkingDistance = 3f;
            maxCyclingDistance = 12f;
            Time = 0;
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
            Time++;
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
            {
                return driving.d(start, end) + 2 * carParkingCost + walking.d(start,end)*carEmissions;
            }
            else if (vehicle is Bike)
            {
                if (walking.d(start, end) < maxCyclingDistance)
                    return cycling.d(start, end) + 2 * bikeParkingCost;
                else
                    return 1000000; //MAGIC NUMBERS
            }
            else
            {
                if (walking.d(start, end) < maxWalkingDistance)
                    return walking.d(start, end);
                else
                    return 1000000; //MOAR MAGIC NUMBERS
            }
                
        }
        private void executeLog(Log log)
        {
            List<Node> path = log.path;
            log.person.move(path[path.Count-1], nodes);
            List<Vehicle> vehicles = log.vehicles;
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (!(vehicles[i] is Legs))
                {
                    vehicles[i].move(path[i + 1], nodes);
                    if (vehicles[i] is Car) carUsage++;
                    if (vehicles[i] is Bike) bikeUsage++;
                }
                else if (vehicles[i] is Legs) legsUsage++;
            }
        }
        private void updatePushPull()
        {
            // Residential areas
            int third = nodes.Length / 3;
            for (int i = 0; i < third; i++)
            {
                if (Time <= 5 && Time >= 21)
                {
                    pull[i] = random.Next(5,10);
                    push[i] = random.NextDouble();
                }
                else if (Time >= 9 && Time <= 17)
                {
                    pull[i] = random.Next(2);
                    push[i] = random.NextDouble();
                }
                else
                {
                    pull[i] = random.Next(3,6);
                    push[i] = random.NextDouble();
                }
            }

            for (int i = third; i < third * 2; i++)
            {
                if (Time <= 5 && Time >= 21)
                {
                    pull[i] = random.Next(1);
                    push[i] = random.NextDouble();
                }
                else if (Time >= 9 && Time <= 17)
                {
                    pull[i] = random.Next(6,10);
                    push[i] = random.NextDouble();
                }
                else
                {
                    pull[i] = random.Next(3);
                    push[i] = random.NextDouble();
                }
            }
            for (int i = third*2; i < nodes.Length; i++)
            {
                if (Time <= 3 && Time >= 18)
                {
                    pull[i] = random.Next(5,9);
                    push[i] = random.NextDouble();
                }
                else if (Time >= 9 && Time <= 17)
                {
                    pull[i] = random.Next(3,5);
                    push[i] = random.NextDouble();
                }
                else
                {
                    pull[i] = random.Next(1);
                    push[i] = random.NextDouble();
                }
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
                printable += "\n" + "Time is " + World.Time;
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
                        for (int j = 0; j < carParkNodes.Count; j++)
                        {
                            int endIndex = carParkNodes[j].index;
                            start.Add(vehicle.location);
                            end.Add(endIndex);
                            lengths.Add(effortCost(vehicle.location, endIndex,vehicle));
                        }
                    //cycling to bikePark
                    if (vehicle is Bike)
                        for (int j = 0; j < bikeParkingNodes.Count; j++)
                        {
                            int endIndex = bikeParkingNodes[j].index;
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
