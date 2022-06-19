using RoadProject_SO.Controllers;
using RoadProject_SO.ViewModel;
using System.Collections.Generic;
using System.Windows;

namespace RoadProject_SO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow? GetMain;
        public static List<Controller>? threads;

        public MainWindow()
        {
            InitializeComponent();

            GetMain = this;

            PublicAvaliableReferences.Initialize(Canvas);
            CreateThreads();
            StartThreads();

            this.Closing += MainWindow_Closing;
        }

        #region Threads
        private static void CreateThreads()
        {
            threads = new List<Controller>
            {
                new CarsController(),
                new TrainsController(),
                new TurnpikesAndLightsController()
            };
        }

        private static void AbortThreads()
        {
            foreach (Controller controller in threads)
                controller.Abort();
        }

        private static void StartThreads()
        {
            foreach (Controller controller in threads)
                controller.Start();
        }
        #endregion

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AbortThreads();
        }
    }
}

