using System;
using System.Collections.Generic;
using System.Linq;
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
            bikePark = false;
            carPark = false;
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
            }
            for (int i = 0; i < bikesAmount; i++)
            {
                bikes.Add(new Bike(index));
            }
        }
    }
    public class Person
    {
        public int location;
        public Person(int location)
        {
            this.location = location;
        }
        List<Car> cars;
        List<Bike> bikes;
        public void addCar(Car car)
        {
            cars.Add(car);
            car.owners.Add(this);
        }
        public void addBike(Bike bike)
        {
            bikes.Add(bike);
            bike.owners.Add(this);
        }
        public List<int> getCars()
        {
            List<int> carLocations = new List<int>();
            for (int i = 0; i < cars.Count; i++)
                carLocations.Add(cars[i].location);
            return carLocations;
        }
        public List<int> getBikes()
        {
            List<int> bikeLocations = new List<int>();
            for (int i = 0; i < bikes.Count; i++)
                bikeLocations.Add(bikes[i].location);
            return bikeLocations;
        }
    }
    public class Vehicle
    {
        public List<Person> owners;
        public int location;
    }
    public class Bike : Vehicle
    {
        public Bike(int location)
        {
            this.location = location;
        }
    }
    public class Car : Vehicle
    {
        public Car(int location)
        {
            this.location = location;
        }
    }
}
