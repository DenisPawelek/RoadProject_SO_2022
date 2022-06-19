using RoadProject_SO.ViewModel;
using System.Threading;

namespace RoadProject_SO.Controllers
{
    class TrainsController : Controller
    {
        protected override void RunThread()
        {
            do
            {
                PublicAvaliableReferences.UpdateAllTrains();
                Thread.Sleep(THREAD_TICK);
            } while (true);
        }
    }
}
