using RoadProject_SO.Model;
using RoadProject_SO.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RoadProject_SO.ViewModel
{
    public class PublicAvaliableReferences
    {
        public const float TICK_VALUE = 1.0f;
        public const int ALPHA = 255;

        //public static bool IsCarPoolFinished { get; protected set; }
        public static Factory factory = new Factory();

        #region Cars
        public static bool DrawCars { get; set; } = true;
        public static List<Car> cars;
        public static List<Node> carNodes;
        protected static List<Image> carsArt;

        public const string CAR_RESOURCES_FOLDER = @"\Resources\Images\Cars\";
        public const string CAR_IMAGE_PREFIX = "car_";

        protected const int SPAWN_CAR_LIMIT = 14;
        protected const int CAR_DIRECTIONS = 8;

        protected static BitmapImage[] carsBitmaps = new BitmapImage[8];
        #endregion

        #region Trains
        public static bool DrawTrains { get; set; } = true;
        public static List<Train> trains;
        protected static List<Image> trainsArt;
        protected static List<Node> trainNodes;

        public const string TRAIN_RESOURCES_FOLDER = @"\Resources\Images\Trains\";
        public const string TRAIN_IMAGE_PREFIX = "train";

        //protected const int SPAWN_TRAIN_LIMIT = 1;

        protected static BitmapImage[] trainsBitmaps = new BitmapImage[1];
        #endregion

        #region Lights and Turnpikes

        public static List<Light> lights;
        protected static List<Image> lightsArt;
        public const string LIGHT_RESOURCES_FOLDER = @"\Resources\Images\Objects\Lights\";
        public const string LIGHT_IMAGE_PREFIX = "lights_";

        public static List<Turnpike> turnpikes;
        protected static List<Image> turnpikesArt;
        public const string TURNPIKE_RESOURCES_FOLDER = @"\Resources\Images\Objects\Turnpikes\";
        public const string TURNPIKE_IMAGE_PREFIX = "turnpike_";

        #endregion

        protected static Canvas? canvas;

        protected static int RailsNodeIndexTop { get; set; } = 21;
        protected static int RailsNodeIndexBottom { get; set; } = 11;

        private static readonly float[,] DIRECTIONS = 
        {
            // Y value increases the lower object is on canvas - Y value is inversed

            //X-min      X-max      Y-min       Y-max
            {-.25f,     0.25f,      -.75f,       -1.0f},       // UP
            {0.00f,     0.75f,      -.00f,       -.75f},       // UP_RIGHT
            {0.75f,     1.0f,       0.25f,       -.25f},       // RIGHT
            {0.00f,     0.75f,      0.00f,       0.75f},       // DOWN_RIGHT
            {-.25f,     0.25f,      0.75f,       1.00f},       // DOWN
            {-.75f,     0.00f,      0.00f,       0.75f},       // DOWN_LEFT
            {-1.0f,     -.75f,      0.25f,       -.25f},       // LEFT
            {-.75f,     0.00f,      -.00f,       -.75f},       // UP_LEFT
        };


        #region Initialization

        public static void Initialize(Canvas passedCanvas)
        {
            canvas = passedCanvas;
            //IsCarPoolFinished = false;
            CreateBitmapImages();
            CreateNodes();
            CreatePools();
        }

        /// <summary>
        /// Creates all <see cref="List{T}"/> of <see cref="Node"/>
        /// </summary>
        private static void CreateNodes()
        {
            CreateCarsNodes();
            CreateTrainNodes();
        }

        /// <summary>
        /// Creates all <see cref="List{T}"/> of <see cref="Vehicle"/>, <see cref="Turnpike"/>, <see cref="Light"/>
        /// </summary>
        private static void CreatePools()
        {
            CreateCarsPool();
            CreateTrainsPool();
            CreateLightsPool();
            CreateTurnpikesPool();
        }

        #region Nodes Creation

        private static void CreateCarsNodes()
        {
            if (carNodes is null)
                carNodes = new List<Node>();

            Car.FullTravelDistance = 0;

            List<Ellipse> canvasNodes = new List<Ellipse>();
            foreach (Object ob in canvas.Children)
            {
                if (ob is Ellipse)
                    canvasNodes.Add(ob as Ellipse);
            }

            foreach (Ellipse el in canvasNodes)
            {
                if (el.Tag.Equals("C"))
                {
                    double _xValue = Canvas.GetLeft(el);
                    double _yValue = Canvas.GetTop(el);
                    carNodes.Add(new Node(new Point(_xValue, _yValue)));
                }
            }

            //Calculate Vector for all nodes
            for (int i = 0; i < carNodes.Count; i++)
            {
                Node _node = carNodes[i];
                if (i + 1 >= carNodes.Count)
                {
                    // don't go to the last Node; casues a weird "move across screen" bug
                    _node.CanGoTo = false;
                    _node.CalculateVector(carNodes[i]);
                }
                else
                    _node.CalculateVector(carNodes[i + 1]);
                //Sum of the vector lengths
                Car.FullTravelDistance += _node.Vector.Length;
            }
        }

        private static void CreateTrainNodes()
        {
            if (trainNodes is null)
                trainNodes = new List<Node>();

            List<Ellipse> canvasNodes = new List<Ellipse>();
            foreach (Object ob in canvas.Children)
            {
                if (ob is Ellipse)
                    canvasNodes.Add(ob as Ellipse);
            }

            foreach (Ellipse el in canvasNodes)
            {
                if (el.Tag.Equals("T"))
                {
                    double _xValue = Canvas.GetLeft(el);
                    double _yValue = Canvas.GetTop(el);
                    trainNodes.Add(new Node(new Point(_xValue, _yValue)));
                }
                else if (el.Tag.Equals("TT"))
                {
                    double _xValue = Canvas.GetLeft(el);
                    double _yValue = Canvas.GetTop(el);
                    trainNodes.Add(new TrainTriggerNode(new Point(_xValue, _yValue)));
                }
            }

            //Calculate Vector for all nodes
            for (int i = 0; i < trainNodes.Count; i++)
            {
                Node _node = trainNodes[i];
                if (i + 1 >= trainNodes.Count)
                {
                    _node.CalculateVector();
                }
                else
                    _node.CalculateVector(trainNodes[i + 1]);
            }
        }

        #endregion

        #region Bitmaps
        /// <summary>
        /// Import Bitmaps from Resources
        /// </summary>
        private static void CreateBitmapImages()
        {
            string _defaultResourcesFolder = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(path: Directory.GetCurrentDirectory()));
            string _fileExtension = ".png";
            string _path;

            //Cars
            _path = $"{_defaultResourcesFolder}{CAR_RESOURCES_FOLDER}{CAR_IMAGE_PREFIX}";
            for (int i = 0; i < CAR_DIRECTIONS; i++)
               // carsBitmaps[i] = new BitmapImage(new Uri($"{_path}{i}{_fileExtension}"));
                carsBitmaps[i] = new BitmapImage(new Uri($"C:\\Users\\hindu\\source\\repos\\RoadProject_SO_2022\\RoadProject_SO\\Resources\\Images\\Cars\\car_"+Convert.ToString(i)+".png"));
            


            //Trains
            _path = $"{_defaultResourcesFolder}{TRAIN_RESOURCES_FOLDER}{TRAIN_IMAGE_PREFIX}";
          //trainsBitmaps[0] = new BitmapImage(new Uri($"{_path}{_fileExtension}"));
           trainsBitmaps[0] = new BitmapImage(new Uri($"C:\\Users\\hindu\\source\\repos\\RoadProject_SO_2022\\RoadProject_SO\\Resources\\Images\\Trains\\train.png"));


            //Lights
            _path = $"{_defaultResourcesFolder}{LIGHT_RESOURCES_FOLDER}{LIGHT_IMAGE_PREFIX}";
            //Light.lightsOff = new BitmapImage(new Uri($"{_path}{0}{_fileExtension}"));
            Light.lightsOff = new BitmapImage(new Uri($"C:\\Users\\hindu\\source\\repos\\RoadProject_SO_2022\\RoadProject_SO\\Resources\\Images\\Objects\\Lights\\lights_0.png"));
           //Light.lightsOn[0] = new BitmapImage(new Uri($"{_path}{1}{_fileExtension}"));
            Light.lightsOn[0] = new BitmapImage(new Uri($"C:\\Users\\hindu\\source\\repos\\RoadProject_SO_2022\\RoadProject_SO\\Resources\\Images\\Objects\\Lights\\lights_1.png"));
            //Light.lightsOn[1] = new BitmapImage(new Uri($"{_path}{2}{_fileExtension}"));
            Light.lightsOn[1] = new BitmapImage(new Uri($"C:\\Users\\hindu\\source\\repos\\RoadProject_SO_2022\\RoadProject_SO\\Resources\\Images\\Objects\\Lights\\lights_2.png"));

            //Turnpikes
            _path = $"{_defaultResourcesFolder}{TURNPIKE_RESOURCES_FOLDER}{TURNPIKE_IMAGE_PREFIX}";
            //Turnpike.TurnpikeGraphic[0, 0] = new BitmapImage(new Uri($"{_path}{0}_{0}{_fileExtension}"));
            Turnpike.TurnpikeGraphic[0, 0] = new BitmapImage(new Uri($"C:\\Users\\hindu\\source\\repos\\RoadProject_SO_2022\\RoadProject_SO\\Resources\\Images\\Objects\\Turnpikes\\turnpike_0_0.png"));
           //Turnpike.TurnpikeGraphic[0, 1] = new BitmapImage(new Uri($"{_path}{0}_{1}{_fileExtension}"));
            Turnpike.TurnpikeGraphic[0, 1] = new BitmapImage(new Uri($"C:\\Users\\hindu\\source\\repos\\RoadProject_SO_2022\\RoadProject_SO\\Resources\\Images\\Objects\\Turnpikes\\turnpike_0_1.png"));
            //Turnpike.TurnpikeGraphic[1, 0] = new BitmapImage(new Uri($"{_path}{1}_{0}{_fileExtension}"));
            Turnpike.TurnpikeGraphic[1, 0] = new BitmapImage(new Uri($"C:\\Users\\hindu\\source\\repos\\RoadProject_SO_2022\\RoadProject_SO\\Resources\\Images\\Objects\\Turnpikes\\turnpike_1_0.png"));
            //Turnpike.TurnpikeGraphic[1, 1] = new BitmapImage(new Uri($"{_path}{1}_{1}{_fileExtension}"));
            Turnpike.TurnpikeGraphic[1, 1] = new BitmapImage(new Uri($"C:\\Users\\hindu\\source\\repos\\RoadProject_SO_2022\\RoadProject_SO\\Resources\\Images\\Objects\\Turnpikes\\turnpike_1_1.png"));
        }

        //XD
        #endregion

        #region Pools Creation

        private static void CreateTrainsPool()
        {
            int nodesCount = trainNodes.Count();
            if (nodesCount <= 0)
                throw new Exception();

            trains = new List<Train>();
            trainsArt = new List<Image>();

            Train train = factory.CreateTrain(nodesCount, -1);

            //set first node as train's stariting position
            train.ActualPosition = trainNodes[0].GetNodePosition();

            //assign first grahpic to the train
            train.CurrentGraphics = trainsBitmaps[0];

            trains.Add(train);

            Image trainImage = new Image
            {
                Width = Train.TRAIN_WIDTH,
                Height = Train.TRAIN_HEIGHT,
                Source = train.CurrentGraphics
            };

            trainsArt.Add(trainImage);
            Panel.SetZIndex(trainImage, 7); // between 5 and 10 to fit in between TrunPikes and Lights
            canvas.Children.Add(trainImage);

            MainWindow.GetMain.Dispatcher.Invoke(UpdateOnCanvas(trainsArt[0], train));

        }

        private static void CreateCarsPool()
        {
            int nodesCount = carNodes.Count();
            if (nodesCount <= 0)
                throw new Exception();

            cars = new List<Car>();
            carsArt = new List<Image>();

            for (int i = 0; i < SPAWN_CAR_LIMIT; i++)
            {
                int nextIndex = i - 1 < 0 ? SPAWN_CAR_LIMIT - 1 : i - 1;

                Car car = factory.CreateCar(nodesCount, nextIndex);

                //set first node as car's stariting position
                car.ActualPosition = carNodes[0].GetNodePosition();

                //assign first grahpic to the car
                car.CurrentGraphics = carsBitmaps[(int)Enums.GraphicDirection.LEFT];

                if (i != 0)
                {
                    car.EnableVehicle();
                    car.SpawnDelay = 200 * i;
                }

                cars.Add(car);

                Image carImage = new Image
                {
                    Width = Car.CAR_WIDTH,
                    Height = Car.CAR_HEIGHT,
                    Source = car.CurrentGraphics
                };

                carsArt.Add(carImage);
                Panel.SetZIndex(carImage, 5);
                canvas.Children.Add(carImage);

                MainWindow.GetMain.Dispatcher.Invoke(UpdateOnCanvas(carsArt[i], car));
            }
        }

        private static void CreateTurnpikesPool()
        {
            if (turnpikes is null)
                turnpikes = new List<Turnpike>();
            if (turnpikesArt is null)
                turnpikesArt = new List<Image>();

            Turnpike _turnpikeTop = factory.CreateTurnpike(new Point(550, 150), true);
            Turnpike _turnpikeBottom = factory.CreateTurnpike(new Point(550, 200));

            Image _turnpikeTopImage = new Image
            {
                Source = _turnpikeTop.CurrentGraphic,
                Width = Turnpike.TURNPIKE_WIDTH,
            };
            Image _turnpikeBottomImage = new Image
            {
                Source = _turnpikeBottom.CurrentGraphic,
                Width = Turnpike.TURNPIKE_WIDTH,
            };

            turnpikes.Add(_turnpikeTop);
            turnpikes.Add(_turnpikeBottom);

            turnpikesArt.Add(_turnpikeTopImage);
            turnpikesArt.Add(_turnpikeBottomImage);

            canvas.Children.Add(_turnpikeTopImage);
            canvas.Children.Add(_turnpikeBottomImage);

            Canvas.SetLeft(_turnpikeTopImage, _turnpikeTop.ActualPosition.X);
            Canvas.SetTop(_turnpikeTopImage, _turnpikeTop.ActualPosition.Y);
            Canvas.SetZIndex(_turnpikeTopImage, 5);

            Canvas.SetLeft(_turnpikeBottomImage, _turnpikeBottom.ActualPosition.X);
            Canvas.SetTop(_turnpikeBottomImage, _turnpikeBottom.ActualPosition.Y);
            Canvas.SetZIndex(_turnpikeBottomImage, 10); //has to be higher, because it's "closer"
        }

        private static void CreateLightsPool()
        {
            if (lights is null)
                lights = new List<Light>();
            if (lightsArt is null)
                lightsArt = new List<Image>();

            Light _lightsTop = factory.CreateLight(new Point(510, 165));
            Light _lightsBottom = factory.CreateLight(new Point(650, 220));

            Image _lightsTopImage = new Image
            {
                Source = _lightsTop.CurrentGraphic,
                Width = Light.LIGHT_WIDTH,
            };
            Image _lightsBottomImage = new Image
            {
                Source = _lightsBottom.CurrentGraphic,
                Width = Light.LIGHT_WIDTH,
            };

            lights.Add(_lightsTop);
            lights.Add(_lightsBottom);

            lightsArt.Add(_lightsTopImage);
            lightsArt.Add(_lightsBottomImage);

            canvas.Children.Add(_lightsTopImage);
            canvas.Children.Add(_lightsBottomImage);

            Canvas.SetLeft(_lightsTopImage, _lightsTop.ActualPosition.X);
            Canvas.SetTop(_lightsTopImage, _lightsTop.ActualPosition.Y);
            Panel.SetZIndex(_lightsTopImage, 5);

            Canvas.SetLeft(_lightsBottomImage, _lightsBottom.ActualPosition.X);
            Canvas.SetTop(_lightsBottomImage, _lightsBottom.ActualPosition.Y);
            Panel.SetZIndex(_lightsBottomImage, 10); //has to be higher because it's "closer"
        }
        #endregion

        #endregion

        #region Updates

        public static void UpdateAllTrains()
        {
            lock (trains) lock (trainsArt)
                {
                    for (int i = 0; i < trains.Count; i++)
                    {
                        Train train = trains[i];

                        //if train is not active, revive it after 3 sec
                        if (!train.IsActive)
                        {
                            Thread.Sleep(3000);
                            ReincarnateTrain(train);
                        }

                        train.UpdateVehicle();

                        if (!train.IsVisible || !DrawTrains)
                            continue;

                        if (MainWindow.GetMain != null)
                            MainWindow.GetMain.Dispatcher.Invoke(UpdateOnCanvas(trainsArt[i], train));
                    }
                }
        }

        public static void UpdateAllCars()
        {
            lock (cars) lock (carsArt)
                {
                    for (int i = 0; i < cars.Count; i++)
                    {
                        Car _car = cars[i];
                 



                        

                        //if car is not active, revive it after SpawnDelay time
                        if (!_car.IsActive)
                        {
                            _car.SpawnDelay -= 1.0f;
                            if (_car.SpawnDelay <= 0.0f)
                            {
                                ReincarnateCar(_car);
                            }
                            continue;
                        }

                        _car.UpdateVehicle();

                        if (!_car.IsVisible || !DrawCars)
                            continue;

                        if (MainWindow.GetMain != null)
                            MainWindow.GetMain.Dispatcher.Invoke(UpdateOnCanvas(carsArt[i], _car));
                    }
                }
        }

        public static void UpdateAllTurnpikes(bool turnpikeStatus)
        {
            lock (turnpikes) lock (turnpikesArt)
                {
                    for (int i = 0; i < turnpikes.Count; i++)
                    {
                        Turnpike _turnpike = turnpikes[i];

                        //if there is a car on the railway, do not close the turnpike
                        if (IsCarOnRailWay())
                            continue;

                        _turnpike.Opened = turnpikeStatus;
                        _turnpike.Update();

                        if (MainWindow.GetMain != null)
                            MainWindow.GetMain.Dispatcher.Invoke(UpdateOnCanvas(turnpikesArt[i], _turnpike.CurrentGraphic));
                    }
                }
        }

        public static void UpdateAllLights(bool turnpikeStatus)
        {
            lock (lights) lock (lightsArt)
                {
                    for (int i = 0; i < lights.Count; i++)
                    {
                        Light _light = lights[i];
                        _light.SetStatus(turnpikeStatus);
                        _light.Update();

                        if (MainWindow.GetMain != null)
                            MainWindow.GetMain.Dispatcher.Invoke(UpdateOnCanvas(lightsArt[i], _light.CurrentGraphic));
                    }
                }
        }

        private static bool IsCarOnRailWay()
        {
            lock (cars)
            {
                bool _isCarOnRailway = false;
                for (int i = 0; i < cars.Count; i++)
                {
                    Car _car = cars[i];

                    //if car's current Node is not railway node, skip the car
                    if (_car.NodesLeftToTravel != (carNodes.Count - RailsNodeIndexTop + 1) && _car.NodesLeftToTravel != (carNodes.Count - RailsNodeIndexBottom + 1))
                    {
                        continue;
                    }

                    //if car is 10% on the path to the railway, don't let it stop and don't let the turnpikes to close
                    if (_car.GetRelativeDistanceTravelRatio() > 0.1f)
                    {
                        _car.IgnoreCanGoThrough();
                        _isCarOnRailway = true;
                    }
                }
                return _isCarOnRailway;
            }
        }


        private static Action UpdateOnCanvas(Image image, BitmapImage newImage)
        {
            return () =>
            {
                image.Source = newImage;
            };
        }

        private static Action UpdateOnCanvas(Image image, Vehicle vehicle)
        {
            return () =>
            {
                image.Source = vehicle.CurrentGraphics;
                Canvas.SetLeft(image, vehicle.ActualPosition.X);
                Canvas.SetTop(image, vehicle.ActualPosition.Y);
            };
        }

        #endregion

        #region Reincarnation

        public static void ReincarnateTrain(Train train)
        {
            lock (train)
            {
                train.NodesLeftToTravel = trainNodes.Count;
                train.ResetPosition();
                train.UpdatePositionVector(train.NodesLeftToTravel);
                train.CanMove = true;
                train.IsActive = true;
            }
        }

        private static void ReincarnateCar(Car car)
        {
            car.IsActive = true;

            int distance;
            
            Random random = new();
            if (random.Next(0, 10) % 2 == 0)
            {
                car.NodesLeftToTravel = carNodes.Count;
                distance = 0;
        }
            else
            {
                distance = 0;
                car.NodesLeftToTravel = carNodes.Count/2;
            }

    car.VehicleSpeed = random.NextDouble() * 2.0f + 1;
            car.ResetPosition(distance);
            car.EnableVehicle();
        }

        #endregion

        #region Checks



        private static Vehicle GetNextCar(Vehicle lastvehicle)
        {
            Vehicle nextvehicle = lastvehicle;

            //int nodes = lastvehicle.NodesLeftToTravel;
            if (lastvehicle.NodesLeftToTravel >= 16)
            {
                foreach (var a in cars)
                {
                    // a.NodesLeftToTravel
                    if (a.TraveledDistance > lastvehicle.TraveledDistance )
                    {
                        if (nextvehicle == lastvehicle && a.NodesLeftToTravel >= 16)
                        {
                            nextvehicle = a;
                        }
                        else if (a.TraveledDistance < nextvehicle.TraveledDistance && a.NodesLeftToTravel >= 16)
                        {
                            nextvehicle = a;
                        }
                    }
                }

            }
            else
            {
                foreach (var a in cars)
                {

                    // a.NodesLeftToTravelll

                    // a.NodesLeftToTravel

                    if (a.TraveledDistance > lastvehicle.TraveledDistance)
                    {
                        if (nextvehicle == lastvehicle && a.NodesLeftToTravel < 16)
                        {
                            nextvehicle = a;
                        }
                        else if (a.TraveledDistance < nextvehicle.TraveledDistance && a.NodesLeftToTravel <16)
                        {
                            nextvehicle = a;
                        }
                    }
                }

            }



            return nextvehicle;
        }


        public static bool IsAnyVehicleInFront(Vehicle thisVehicle)
        {

            lock (cars)
            {
                if (!BasicChecksForVehicle(thisVehicle))
                {
                    if (thisVehicle.NextVehicleIndex <= -1)
                    {
                        Vehicle nextCar = GetNextCar(thisVehicle);
                        bool vehicleExists = nextCar.IsActive && nextCar.CanColide && !nextCar.Arived();
                        return vehicleExists;

                    }
                    return false;
                }


                else
                {
                    Vehicle nextCar = GetNextCar(thisVehicle);
                    bool vehicleExists = nextCar.IsActive && nextCar.CanColide && !nextCar.Arived();
                    return vehicleExists;
                }

               
            }
        }

        public static bool IsVehicleInTheWay(Vehicle thisVehicle)
        {
            //if (thisVehicle is Train)
            //    return false;

            lock (cars)
            {
                if (!BasicChecksForVehicle(thisVehicle))
                {
                    if (thisVehicle.NextVehicleIndex <= -1)
                    {
                        Vehicle nextCar2 = GetNextCar(thisVehicle);
                        double nextVehicleBack2 = nextCar2.TraveledDistance;
                        double thisVehicleFront2 = thisVehicle.TraveledDistance;
                        double differenceInDistance2 = Math.Abs(nextVehicleBack2 - thisVehicleFront2);
                        bool areTooClose2 = differenceInDistance2 < Vehicle.VEHICLE_DISTANCE_OFFSET;
                        return areTooClose2;

                    }
                    return false;
                }

                Vehicle nextCar = GetNextCar(thisVehicle);

                //calculate distance between two vehicles
                double nextVehicleBack = nextCar.TraveledDistance;
                double thisVehicleFront = thisVehicle.TraveledDistance;
                double differenceInDistance = Math.Abs(nextVehicleBack - thisVehicleFront);
                bool areTooClose = differenceInDistance < Vehicle.VEHICLE_DISTANCE_OFFSET;
                return areTooClose;
            }
        }



        public static bool CanVehicleOvertake(Vehicle thisVehicle)
        {
            //thisVehicle.
           


            if (thisVehicle.NodesLeftToTravel > 16)
            {
                foreach (var opositecar in cars)
                {
                    if (opositecar.NodesLeftToTravel < 15)
                    {
                        if (Math.Abs(opositecar.ActualPosition.X - thisVehicle.ActualPosition.X) > 50 && Math.Abs(opositecar.ActualPosition.Y - thisVehicle.ActualPosition.Y) > 10)
                        {


                        }
                        else
                        {
                            return false;
                        }

                    }
                }
                return true;

            }

            else
            {

                foreach (var opositecar in cars)
                {
                    if (opositecar.NodesLeftToTravel > 15)
                {
                    if (Math.Abs(opositecar.ActualPosition.X - thisVehicle.ActualPosition.X) > 50 && Math.Abs(opositecar.ActualPosition.Y - thisVehicle.ActualPosition.Y) > 10)
                    {


                    }
                    else
                    {
                        return false;
                    }

                }

                    }
                return true;

            }

                return true;
        }



        /// <summary>
        /// Checks if the next Vehicle is valid
        /// </summary>
        /// <param name="vehicle">Vehicle to check</param>
        /// <returns>true if basics checks are fine</returns>
        private static bool BasicChecksForVehicle(Vehicle vehicle)
        {
            //first vehicle
            if (vehicle.NextVehicleIndex <= -1)
                return false;

            Vehicle nextCar;
            nextCar = cars[vehicle.NextVehicleIndex];

            //if it's the same vehicle
            if (vehicle.NextVehicleIndex == nextCar.NextVehicleIndex)
                return false;

            //if it's working vehicle
            if (!nextCar.IsActive || !nextCar.CanColide || nextCar.Arived())
                return false;

            return true;
        }

        private static bool IsInBetween(double value, float smallerLimit, float biggerLimit)
        {
            if (smallerLimit > biggerLimit)
            {
                float temp = smallerLimit;
                smallerLimit = biggerLimit;
                biggerLimit = temp;
            }

            bool isInBetweenValues = (value >= smallerLimit & value <= biggerLimit);
            return isInBetweenValues;
        }

        public static void TriggerTurnPike()
        {
            lock (carNodes)
            {
                carNodes[RailsNodeIndexTop].CanGoTo = !carNodes[RailsNodeIndexTop].CanGoTo;
                carNodes[RailsNodeIndexBottom].CanGoTo = !carNodes[RailsNodeIndexBottom].CanGoTo;
            }
        }
        #endregion

        #region Gets

        public static Node GetCarNode(int nodesLeftToTravel) => GetNode(carNodes, nodesLeftToTravel);
        public static Node GetTrainNode(int nodesLeftToTravel) => GetNode(trainNodes, nodesLeftToTravel);

        public static Node GetNode(List<Node> nodesArray, int rawNodesLeftToTravel)
        {
            lock (nodesArray)
            {
                //rawNodesLeftToTravel are indexed from 0, while the array is indexed from 0
                rawNodesLeftToTravel--;

                //rawNodesLeftToTravel are inversed
                int _nodesCount = nodesArray.Count() - 1;
                int _currentNodeIndex = _nodesCount - rawNodesLeftToTravel;

                if (_currentNodeIndex < 0)
                    return null;

                if (_currentNodeIndex > nodesArray.Count() - 1)
                    return null;

                return nodesArray[_currentNodeIndex];
            }
        }

        public static BitmapImage GetNextCarGraphic(double normalizedX, double normalizedY)
        {
            lock (DIRECTIONS)
            {
                bool _determined = false; //did search loop found matching direction

                // set default as UP_RIGHT direction (easiest to spot for errors in the search alg)
                int _selectedDirection = (int)Enums.GraphicDirection.UP_RIGHT;

                for (int i = 0; i < CAR_DIRECTIONS; i++)
                {
                    //Search for X
                    if (!IsInBetween(normalizedX, DIRECTIONS[i, 0], DIRECTIONS[i, 1]))
                        continue;

                    //Search for Y
                    if (!IsInBetween(normalizedY, DIRECTIONS[i, 2], DIRECTIONS[i, 3]))
                        continue;

                    _selectedDirection = i;
                    _determined = true;
                    break;
                }

                if (_determined)
                    return carsBitmaps[_selectedDirection];
                else
                {
                    // algorithm did not find a proper direction; use RIGHT or LEFT depending on X
                    int _index = normalizedX >= 0.0f ? (int)Enums.GraphicDirection.RIGHT : (int)Enums.GraphicDirection.LEFT;
                    return carsBitmaps[_index];
                }
            }
        }





        public static double GetNextVehicleSpeed(Vehicle vehicle)
        {
            lock (cars)
            {
                Vehicle n = GetNextCar(vehicle);
            
                return n.CurrentSpeed;
            }
        }

        public static bool GetTurnPikeStatus()
        {
            return carNodes[RailsNodeIndexTop].CanGoTo && carNodes[RailsNodeIndexBottom].CanGoTo;
        }
        #endregion
    }
}
