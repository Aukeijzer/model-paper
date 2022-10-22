using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Paper_Model
{
    class WorldNode : Node
    {
        public WorldNode(int index) : base(index)
        {
            people = new List<Person>();
            bikes = new List<Bike>();
            cars = new List<Car>();
            bikePark = true;
            carPark = true;
        }
        public WorldNode(Node node) : this(node.index)
        {
            neighbors = node.neighbors;
            distance2Neighbor = node.distance2Neighbor;
        }
        public List<Person> people;
        public bool bikePark;
        public List<Bike> bikes;
        public bool carPark;
        public List<Car> cars;
        public void addFamily(int peopleAmount, int carAmount, int bikesAmount)
        {
            for (int i = 0; i < peopleAmount; i++)
                people.Add(new Person(index));
            for (int i = 0; i < carAmount; i++)
            {
                cars.Add(new Car(index));
                for (int j = 0; j < peopleAmount; j++)
                    people[j].addVehicle(cars[i]);
            }
            for (int i = 0; i < bikesAmount; i++)
            {
                bikes.Add(new Bike(index));
                for (int j = 0; j < peopleAmount; j++)
                    people[j].addVehicle(bikes[i]);
            }
        }
        public Car useCar(Person person)
        {
            for (int i = 0; i < cars.Count; i++)
                if (cars[i].owners.Contains(person))
                {
                    cars[i].moving = true;
                    return cars[i];
                }
            return default;
        }
        public Bike useBike(Person person)
        {
            for (int i = 0; i < bikes.Count; i++)
                if (bikes[i].owners.Contains(person))
                {
                    bikes[i].moving = true;
                    return bikes[i];
                }
            return default;
        }
    }
    public class Thing
    {
        public int location;
        public bool moving;
        public void move(int index)
        {
            location = index;
            moving = false;
        }
    }
    public class Person
    {
        public int location;
        public bool moving;
        public Person(int location)
        {
            this.location = location;
        }
        List<Car> cars = new List<Car>();
        List<Bike> bikes = new List<Bike>();
        public void addVehicle(Vehicle vehicle)
        {
            if  (vehicle is Car car)
                cars.Add(car);
            if (vehicle is Bike bike)
                bikes.Add(bike);
            vehicle.owners.Add(this);
        }
        public List<int> getCars()
        {
            //TODO: Carpooling
            List<int> carLocations = new List<int>();
            for (int i = 0; i < cars.Count; i++)
                if(!cars[i].moving)
                    carLocations.Add(cars[i].location);

            return carLocations;
        }
        public List<int> getBikes()
        {
            List<int> bikeLocations = new List<int>();
            for (int i = 0; i < bikes.Count; i++)
                if(!bikes[i].moving)
                    bikeLocations.Add(bikes[i].location);
            return bikeLocations;
        }
    }
    public class Vehicle
    {
        public List<Person> owners = new List<Person>();
        public int location;
        public bool moving = false;
    }
    public class Bike : Vehicle
    {
        public Bike(int location)
        {
            this.location = location;
        }

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
        public override string ToString()
        {
            return "Car";
        }
    }
}
