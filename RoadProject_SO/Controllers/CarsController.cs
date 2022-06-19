using RoadProject_SO.ViewModel;
using System.Threading;

namespace RoadProject_SO.Controllers
{
    class CarsController : Controller
    {
        protected override void RunThread()
        {
            do
            {
                PublicAvaliableReferences.UpdateAllCars();
                Thread.Sleep(THREAD_TICK);
            } while (true);
        }
    }
}
