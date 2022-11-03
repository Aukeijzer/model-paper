using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Paper_Model
{
    public class WorldNode : Node
    {
        public WorldNode(int index) : base(index)
        {
            people = new List<Person>();
            vehicles = new List<Vehicle>();
            bikePark = true;
            carPark = true;
        }
        public WorldNode(Node node) : this(node.index)
        {
            neighbors = node.neighbors;
            distance2Neighbor = node.distance2Neighbor;
            reverseNeighbor = node.reverseNeighbor;
            reverseNeighborDistance = node.reverseNeighborDistance;
        }
        public List<Person> people;
        public bool bikePark;
        public bool carPark;
        private List<Vehicle> vehicles;
        public void addFamily(int peopleAmount, int carAmount, int bikesAmount)
        {
            for (int i = 0; i < peopleAmount; i++)
                people.Add(new Person(index));
            for (int i = 0; i < carAmount; i++)
            {
                vehicles.Add(new Car(index));
                for (int j = 0; j < peopleAmount; j++)
                    people[j].addVehicle(vehicles[i]);
            }
            for (int i = 0; i < bikesAmount; i++)
            {
                vehicles.Add(new Bike(index));
                for (int j = 0; j < peopleAmount; j++)
                    people[j].addVehicle(vehicles[i]);
            }
        }

        public void removeThing(Thing thing)
        {
            if (thing is Person person)
                people.Remove(person);
            if (thing is Vehicle vehicle)
                vehicles.Remove(vehicle);
        }
        public void addThing(Thing thing)
        {
            if (thing is Person person)
                people.Add(person);
            if (thing is Vehicle vehicle)
                vehicles.Add(vehicle);
        }
    }
    public abstract class Thing
    {
        public int location;
        public bool moving = false;
        public void move(Node destination,WorldNode[] nodes)
        {
            move(destination.index, nodes);
        }
        public void move(int destination, WorldNode[] nodes)
        {
            nodes[location].removeThing(this);
            location = destination;
            nodes[location].addThing(this);
            moving = false;
        }
    }
    public class Person : Thing
    {
        public Person(int location)
        {
            this.location = location;
        }
        public Person() { }
        public List<Vehicle> vehicles = new List<Vehicle>();
        public void addVehicle(Vehicle vehicle)
        {
            vehicles.Add(vehicle);
            vehicle.owners.Add(this);
        }
        public List<T> getSpecificVehicle<T>() where T : Vehicle
        {
            List<T> vehicleType = new List<T>();
            for (int i = 0; i < vehicles.Count; i++)
                if (vehicles[i] is T type)
                    vehicleType.Add(type);
            return vehicleType;
        }
    }
    public abstract class Vehicle : Thing
    {
        public List<Person> owners = new List<Person>();
    }
    public class Bike : Vehicle
    {
        public Bike(int location)
        {
            this.location = location;
        }
        public Bike() { }
        public override string ToString()
        {
            return "Bike";
        }
    }
    public class Car : Vehicle
    {
        public Car(int location)
        {
            this.location = location;
        }
        public Car() { }
        public override string ToString()
        {
            return "Car";
        }
    }
    public class Legs : Vehicle
    {
        public override string ToString()
        {
            return "Walk";
        }
    }
    public class Bus : Vehicle
    {
        public override string ToString()
        {
            return "Bus";
        }
    }
}
