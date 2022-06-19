using RoadProject_SO.Model;
using System;
using System.Windows;

namespace RoadProject_SO.Nodes
{
    /// <summary>
    /// A Node is a Abstract object that contains information for vehicles allowing them movement. 
    /// </summary>
    public class Node
    {
        #region Fields
        public const double NODE_SIZE = 20;
        public bool CanGoTo { get; set; }
        protected Point Position { get; set; }
        private bool isActive;
        #endregion

        #region Getter_Setter
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        public PositionVector Vector { get; private set; }
        #endregion

        #region Constructors
        public Node()
        {
            CanGoTo = true;
        }

        public Node(Point position) : this()
        {
            this.Position = position;
        }

        public Point GetNodePosition()
        {
            return this.Position;
        }
        #endregion

        #region Methods
        public void CalculateVector() //funkja obliczająca długość między dwoma node'ami na mapie - obecnym oraz następnym
        {
            CalculateVector(this);
        }

        public void CalculateVector(Node nextNode) //funkja obliczająca długość między dwoma node'ami na mapie - obecnym oraz następnym
        {
            double _xDifferenceSquared;
            double _yDifferenceSquared;
            double _vectorLength;

            //Długość wektora jest obliczana ze wzoru |AB| = PIERWIASTEK[(Xb - Xa)^2 + (Yb - Ya)^2]
            //gdzie B oznacza Node wysłany jako parametr funkcji, zaś A - this.Node
            //finalVector jest zmienną przechowującą długość wektora pomiędzy punktami A i B
            //Wykorzystujemy również znormalizowane długości w celu obliczenia stosunku przesunięcia między node'ami w płaszczyźnie XY

            _xDifferenceSquared = Math.Pow((nextNode.Position.X - this.Position.X), 2);
            _yDifferenceSquared = Math.Pow((nextNode.Position.Y - this.Position.Y), 2);

            _vectorLength = Math.Sqrt(_xDifferenceSquared + _yDifferenceSquared);

            Vector = new PositionVector(Position.X, Position.Y, _vectorLength);

            double _xValue = nextNode.Position.X - this.Position.X;
            double _yValue = nextNode.Position.Y - this.Position.Y;

            //it's the same node
            if (_vectorLength == 0)
                _vectorLength = 1;

            Point _magnitudeVector = new Point
            {
                X = _xValue / _vectorLength,
                Y = _yValue / _vectorLength
            };
            Vector.SetNormalized(_magnitudeVector.X, _magnitudeVector.Y);
        }
        #endregion
        
    }
}
