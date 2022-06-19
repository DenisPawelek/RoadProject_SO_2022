using System;
using System.Windows;

namespace RoadProject_SO.Model
{
    public class Factory
    {
        private Random random = new Random();

        public Train CreateTrain(int nodesCount, int nextVehicleIndex)
        {
            Train _train = new Train(3.5f, nodesCount, nextVehicleIndex);
            return _train;
        }

        public Car CreateCar(int nodesNumber, int nextVehicleIndex)
        {
            Car _car = new Car(random.NextDouble() * 2.0f + 1, nodesNumber, nextVehicleIndex);
            return _car;
        }

        public Turnpike CreateTurnpike(Point position, bool left = false)
        {
            Turnpike _turnpike = new Turnpike(left, position);
            return _turnpike;
        }

        public Light CreateLight(Point position)
        {
            Light _light = new Light(position);
            return _light;
        }
    }
}
