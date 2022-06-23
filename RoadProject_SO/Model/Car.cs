using RoadProject_SO.Nodes;
using RoadProject_SO.ViewModel;
using System.Windows;

namespace RoadProject_SO.Model
{
    public class Car : Vehicle
    {
        #region Constants
        public const double CAR_HEIGHT = 30f;
        public const double CAR_WIDTH = 40f;
        #endregion

        public static double FullTravelDistance = 0.0f;

        public bool IgnoreNodeLimit { get; private set; }
        public double SpawnDelay { get; set; }

        #region Constructors
        public Car(double VehicleSpeed, int CounterNodes, int NextVehicleIndex)
            : base(VehicleSpeed, CounterNodes, NextVehicleIndex)
        {
            EnableVehicle();
            this.positionVector = GetNextNode(CounterNodes).Vector;
            DistanceToTravel = positionVector.Length;
        }
        #endregion

        #region Updates

        public override void UpdateVehicle()
        {
          //  Car _nextcar = cars[NextVehicleIndex];

            if (!IsActive)
                return;

            //There are more nodes outside of Canvas, skipping them for optimalization
            if (NodesLeftToTravel == 2) //|| NodesLeftToTravel == 16)
            {
                //Vehicle Arrived
                DisableVehicle();
                IsActive = false;
            }

            //get next node
            Node _nextNode = GetNextNode(NodesLeftToTravel - 1);

            if (_nextNode == null)
            {
                return;
            }

            GetNewGraphic();

            //if cannot move and doesn't ignore CanGoTo
            if (!CanMove || (!_nextNode.CanGoTo && !IgnoreNodeLimit))
            {
                CurrentSpeed = 0.0f;
                return;
            }

            //Reset speed
            this.CurrentSpeed = this.VehicleSpeed;

            if (CanColide)
                LimitSpeedByVehicleDistance();

            //apply speed to position
            MoveVehicleForward();

            bool _didAriveToNode = (DistanceToTravel - TraveledDistance) <= NODE_DISTANCE_OFFSET;
            if (_didAriveToNode)
            {
                UpdateNode();
                //Reset Node's CanGoTo ignorance
                IgnoreNodeLimit = false;
            }
        }
        #endregion

        #region Gets
        protected override Node GetNextNode(int nodeCount) => PublicAvaliableReferences.GetCarNode(nodeCount);
        #endregion

        #region Sets
        /// <summary>
        /// Resets <see cref="Car"/> position to the first <see cref="Node"/>
        /// </summary>
        public void ResetPosition()
        {
            this.TraveledDistance = 0;
            this.CurrentSpeed = VehicleSpeed;
            this.positionVector = GetNextNode(NodesLeftToTravel).Vector;
            this.DistanceToTravel = positionVector.Length;
            this.ActualPosition = new Point(this.positionVector.X, this.positionVector.Y);
        }

        /// <summary>
        /// Allows Car to ignore next <see cref="Node.CanGoTo"/>
        /// </summary>
        public void IgnoreCanGoThrough()
        {
            IgnoreNodeLimit = true;
        }
        #endregion

    }
}
