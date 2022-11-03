using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms.VisualStyles;
//Alias
using Path = System.Collections.Generic.List<Paper_Model.Node>;

namespace Paper_Model
{
    public class World
    {
        public int Size;
        private WorldNode[] nodes;
        private List<WorldNode> carParkNodes = new List<WorldNode>();
        private List<WorldNode> busLines = new List<WorldNode>();

        private float carEmissions;
        private float bikeParkingCost;
        private float carParkingCost;
        private float busWaitingCost;
        private float busPrice;
        private float gasPrice;
        
        public static int Time;
        
        public static int carUsage;
        public static int bikeUsage;
        public static int legsUsage;
        
        public static float totalCarKM;
        public static float totalCarEmissions;

        //The weight a node has in regards to getting people to move to that node
        private int[] pull;
        //The chance a person has to move away from that node.
        double[] push;
        Random random = new Random();

        private static Graph distances;
        private Graph walking;
        private Graph cycling;
        private Graph driving;

        public World(WorldNode[] nodes, List<List<int>> buslines)
        {
            this.nodes = nodes;
            distances = new Graph(nodes);
            distances.Initialize();
            Graph preWalking = new Graph(distances,1);
            //TODO: calc busCost
            int busCost = 1;
            float busFactor = 1;
            walking = new Graph(preWalking.nodes, buslines, busCost, busFactor, distances);
            walking.Initialize();
            cycling = new Graph(distances,0.3f);
            driving = new Graph(distances,0.02f);
            bikeParkingCost = 1;
            carParkingCost = 10f;
            gasPrice = 0.14f; // 0.14 euro per km
            carEmissions = 0.1f; // 100g CO2 per km
            Time = 0;
            totalCarEmissions = 0f;
            Size = nodes.Length;
            pull = new int[Size];
            push = new double[Size];
            for(int i = 0; i < Size; i++)
                if (nodes[i].carPark)
                    carParkNodes.Add(nodes[i]);

        }
        //still buggy
        public List<Log> Tick()
        {
            updatePushPull();
            Time++;
            if (Time > 24) Time = 1;
            totalCarEmissions = totalCarKM * carEmissions;
            List<Log> logs = movePeople();
            for (int i = 0; i < logs.Count; i++)
                executeLog(logs[i]);
            return logs;
        }
        private float effortCost<T>(int start, int end) where T : Vehicle, new()
        {
            return effortCost(start, end, new T());
        }
        /// <summary>
        /// Looks at the effort cost for going from A to B using a Vehicle
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        private float effortCost(int start, int end, Vehicle vehicle)
        {
            float travelTime; //in minutes
            float physicalEffort;
            float travelDistance = walking.d(start, end);
            float monetaryCost;
            float maxWalkingDistance = 3f;
            float maxCyclingDistance = 10f;
            if (vehicle is Car)
            {
                int parkingTime = 3; //min
                float averageCarLowSpeed = 0.60f; //Average road speed taking into account slower roads (km/min)
                float averageCarSpeed = 0.75f; //Average road speed (km/min)
                travelTime = 2 * parkingTime + 
                             (0.20f*travelDistance * averageCarLowSpeed)+
                             (0.80f*travelDistance * averageCarSpeed);
                monetaryCost = travelDistance * gasPrice+
                               2*carParkingCost;
                physicalEffort = driving.d(start, end);
                return driving.d(start, end) + 2 * carParkingCost + walking.d(start, end)*(gasPrice+carEmissions);
            }
            else if (vehicle is Bike)
            {
                if (travelDistance <= maxCyclingDistance)
                {
                    return cycling.d(start, end) + 2 * bikeParkingCost;
                }
                else
                {
                    return 100000000;
                }
            }
            else if (vehicle is Legs)
            {
                if (travelDistance <= maxWalkingDistance)
                {
                    return travelDistance;
                }
                else
                {
                    return 100000000;
                }
            }
            else
            {
                return 1000000000;
            }
        }
        private void executeLog(Log log)
        {
            Path path = log.path;
            log.person.move(path[path.Count-1], nodes);
            List<Vehicle> vehicles = log.vehicles;
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (!(vehicles[i] is Legs))
                {
                    vehicles[i].move(path[i + 1], nodes);
                    if (vehicles[i] is Car)
                    {
                        carUsage++;
                        totalCarKM = Log.totalKM(log);
                    }
                    if (vehicles[i] is Bike) bikeUsage++;
                }
                else if (vehicles[i] is Legs) legsUsage++;
            }
        }
        private void updatePushPull()  //Whole function is magic numbers (except peak hours)
        {
            // Residential areas
            int third = nodes.Length / 3;
            
            for (int i = 0; i < third; i++)
            {
                
                if (Time <= 5 && Time > 17)
                {
                    pull[i] = random.Next(5,10);
                    push[i] = random.NextDouble();// - 0.2;
                }
                else if (Time >= 6 && Time <= 9)
                {
                    pull[i] = random.Next(2);
                    push[i] = random.NextDouble();// + 0.6;
                }
                else if (Time > 9 && Time <= 17)
                {
                    pull[i] = random.Next(3);
                    push[i] = random.NextDouble();
                }
                
            }
            // Industrial area
            for (int i = third; i < third * 2; i++)
            {
                if (Time <= 5 && Time >= 18)
                {
                    pull[i] = random.Next(2);
                    push[i] = random.NextDouble();// + 0.3;
                }
                else if (Time >= 6 && Time <= 9)
                {
                    pull[i] = random.Next(7, 10);
                    push[i] = random.NextDouble();// - 0.5;
                }
                else if (Time > 9 && Time < 17)
                {
                    pull[i] = random.Next(4,7);
                    push[i] = random.NextDouble();// - 0.1;
                }
            }
            // Recreational Area
            for (int i = third*2; i < nodes.Length; i++)
            {
                if (Time <= 3 && Time > 18)
                {
                    pull[i] = random.Next(5,9);
                    push[i] = random.NextDouble();// - 0.2;
                }
                else if (Time >= 10 && Time <= 18)
                {
                    pull[i] = random.Next(3,5);
                    push[i] = random.NextDouble();// - 0.1;
                }
                else
                {
                    pull[i] = random.Next(2);
                    push[i] = random.NextDouble();// + 0.5;
                }
            }
        }
        public struct Log
        {
            public Person person;
            public Path path;
            public float totalEffort;
            public List<Vehicle> vehicles;
            public List<float> efforts;
            public Log(Path path, float totalEffort, List<Vehicle> vehicles, Person person, List<float> efforts)
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

            public static float totalKM(Log log)
            {
                float totalKM = 0;
                for(int i = 0; i<log.vehicles.Count;i++)
                    if (log.vehicles[i] is Car)
                        totalKM += distances.d(log.path[i], log.path[i+1]);
                return totalKM;
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
                    if (random.NextDouble()-1 < push[i])
                    {
                        int destination = highestCulmative(pull, random.Next(maxPull));
                        logs.Add(movePerson(i, destination, node.people[j]));
                    }
            }
            return logs;
        }
        //TODO: still lots of code duplication in this
        private Graph createEffortGraph(int origin, int destination, Person person)
        {
            List<Vehicle> vehicles = person.vehicles;
            //Keypoints are the destination and nodes which will get you to the destination faster
            List<int> keypoints = new List<int>();
            for (int i = 0; i < vehicles.Count; i++)
                if(!vehicles[i].moving)
                    keypoints.Add(vehicles[i].location);
            for (int i = 0; i < busLines.Count; i++)
                keypoints.Add(busLines[i].index);
            keypoints.Add(destination);

            //Creating new graph
            List<int> start = new List<int>();
            List<int> end = new List<int>();
            List<float> lengths = new List<float>();

            //Walking to keypoints
            for(int i =0; i < keypoints.Count; i++)
            {
                int keypoint = keypoints[i];
                start.Add(origin);
                end.Add(keypoint);
                lengths.Add(effortCost<Legs>(origin, keypoint));
            }
            for(int i = 0; i < vehicles.Count; i++)
            {
                int vehicle = vehicles[i].location;
                //Driving to carParks
                if (vehicles[i] is Car)
                    for(int j = 0; j<carParkNodes.Count;j++)
                    {
                        int carPark = carParkNodes[j].index;
                        start.Add(vehicle);
                        end.Add(carPark);
                        lengths.Add(effortCost<Car>(vehicle, carPark));
                    }    
                //Cycling to other keypoints
                if(vehicles[i] is Bike)
                    for(int j = 0; j < keypoints.Count; j++)
                    {
                        int keypoint = keypoints[j];
                        start.Add(vehicle);
                        end.Add(keypoint);
                        lengths.Add(effortCost<Bike>(vehicle, keypoint));
                    }
            }
            //Walking from keypoints to keypoints (buses force us to do this)
            for(int i = 0; i < keypoints.Count; i++)
                for(int j = 0; j<keypoints.Count;j++)
                {
                    int keyStart = keypoints[i];
                    int keyEnd = keypoints[j];
                    start.Add(keyStart);
                    end.Add(keyEnd);
                    lengths.Add(effortCost<Legs>(keyStart, keyEnd));
                }
            //Walking from carParks to keypoints
            for(int i = 0; i < carParkNodes.Count; i++)
            {
                int carPark = carParkNodes[i].index;
                for(int j = 0; j<keypoints.Count;j++)
                {
                    int keypoint = keypoints[j];
                    start.Add(carPark);
                    end.Add(keypoint);
                    lengths.Add(effortCost<Legs>(carPark, keypoint));
                }
            }
            Node[] nodeArray = Node.createDirectedNodeArray(Size, start.ToArray(), end.ToArray(), lengths.ToArray());
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
        public static float timer = 0;
        private Log movePerson(int origin, int destination, Person person)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Graph effortGraph = createEffortGraph(origin, destination, person);
            stopwatch.Stop();
            timer += stopwatch.ElapsedMilliseconds;
            effortGraph.LazyMinSpanTree(origin, destination);
            float totalEffort = effortGraph.d(origin, destination);
            (List<float> efforts,Path path) = effortGraph.GetPath(origin, destination);
            List<Vehicle> vehicles = new List<Vehicle>();
            for (int i = 0; i<path.Count-1 ; i++)
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
