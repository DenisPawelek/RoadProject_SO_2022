using System.Windows;
using System.Windows.Media.Imaging;

namespace RoadProject_SO.Model
{
    public class Light
    {
        #region Constants
        public const double LIGHT_WIDTH = 80;
        private const int LightsSwitchDelay = 10;
        #endregion

        #region Fields
        public bool Lights { get; set; } = false;
        public bool LeftBlink { get; set; } = false;
        public BitmapImage CurrentGraphic { get; protected set; }
        #endregion

        #region Variables
        public static BitmapImage? lightsOff;
        public static BitmapImage[] lightsOn = new BitmapImage[2];

        public Point ActualPosition;
        private int _lightSwitchCurrent = 0;
        #endregion

        public Light(Point actualPosition)
        {
            ActualPosition = actualPosition;
            UpdateGraphic();
        }

        /// <summary>m
        /// Sets whether <see cref="Light"/> is blinking
        /// </summary>
        /// <param name="status">true if the <see cref="Light"/> is blinking</param>
        public void SetStatus(bool status)
        {
            Lights = !status;
        }

        /// <summary>
        /// Updates Light's status
        /// </summary>
        public void Update()
        {
            UpdateGraphic();
        }

        private void UpdateGraphic()
        {
            if (Lights)
            {
                //10 runs delay to each blink
                _lightSwitchCurrent++;

                if (_lightSwitchCurrent != LightsSwitchDelay)
                    return;

                //Switch light
                CurrentGraphic = LeftBlink ? lightsOn[0] : lightsOn[1];
                LeftBlink = !LeftBlink;
                _lightSwitchCurrent = 0;
            }
            else
                CurrentGraphic = lightsOff;
        }
    }
}

