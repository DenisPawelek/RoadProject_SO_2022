using RoadProject_SO.Nodes;
using RoadProject_SO.ViewModel;

namespace RoadProject_SO.Model
{
    public class Train : Vehicle
    {
        #region Constants

        public const double TRAIN_HEIGHT = 100.0f;
        public const double TRAIN_WIDTH = 150.0f;

        #endregion

        #region Constructors
        public Train(double VehicleSpeed, int CounterNodes, int NextVehicleIndex)
        {
            this.VehicleSpeed = VehicleSpeed;
            this.CurrentSpeed = VehicleSpeed;
            this.NodesLeftToTravel = CounterNodes;
            this.NextVehicleIndex = NextVehicleIndex;
            this.IsActive = true;
            UpdatePositionVector(CounterNodes);
            DistanceToTravel = positionVector.Length;
            ResetPosition();
        }
        #endregion

        #region Updates
        public void UpdatePositionVector(int CounterNodes)
        {
            positionVector = PublicAvaliableReferences.GetTrainNode(CounterNodes).Vector;
        }

        protected override void UpdateNode()
        {
            //reducing count of nodes left
            Node _nextNode = GetNextNode(NodesLeftToTravel - 1);

            if (_nextNode is null)
                return;

            if (!_nextNode.CanGoTo)
                return;

            if (_nextNode is TrainTriggerNode triggerNode)
                triggerNode.TriggerTurnpike();

            //GetNewGraphic();
            NodesLeftToTravel--;

            positionVector = _nextNode.Vector;
            DistanceToTravel += positionVector.Length;
        }
        #endregion

        #region Sets
        /// <summary>
        /// Sets Starting Position of a train to the first Node
        /// </summary>
        public void ResetPosition()
        {
            this.ActualPosition = PublicAvaliableReferences.GetTrainNode(counterNodes - 1).GetNodePosition();
        }

        protected override void SetCounterNodes(int value)
        {
            this.counterNodes = value;
        }
        #endregion

        #region Gets
        //public override void GetNewGraphic() => this.CurrentGraphics = PublicAvaliableReferences.GetNextTrainGraphic(positionVector.NormalizedX, positionVector.NormalizedY);

        protected override Node GetNextNode(int index)
        {
            Node _nextNode = PublicAvaliableReferences.GetTrainNode(index);
            if (_nextNode == null)
                IsActive = false;
            return _nextNode;
        }
        #endregion
    }
}
