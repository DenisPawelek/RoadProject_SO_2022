using System.Windows;
using System.Windows.Media.Imaging;

namespace RoadProject_SO.Model
{

    public class Turnpike
    {
        #region Constants
        public static double TURNPIKE_WIDTH = 140;
        #endregion

        #region Fields
        public BitmapImage CurrentGraphic { get; protected set; }
        public bool Opened { get; set; } = true;
        public bool Left { get; set; } = false;

        #endregion

        #region Variables
        public static BitmapImage[,] TurnpikeGraphic = new BitmapImage[2, 2]; // [Left/Right , Open/Close]
        public Point ActualPosition;
        #endregion

        #region Constructors
        public Turnpike(bool left, Point position)
        {
            this.ActualPosition = position;
            Left = left;
            UpdateGraphic();
        }
        #endregion

        public void Update()
        {
            UpdateGraphic();
        }

        private void UpdateGraphic()
        {
            CurrentGraphic = TurnpikeGraphic[Left ? 0 : 1, Opened ? 0 : 1];
        }
    }
}
