using RoadProject_SO.ViewModel;
using System.Threading;

namespace RoadProject_SO.Controllers
{
    public class TurnpikesAndLightsController : Controller
    {
        protected override void RunThread()
        {
            do
            {
                bool _turnpikeStatus = PublicAvaliableReferences.GetTurnPikeStatus();

                PublicAvaliableReferences.UpdateAllTurnpikes(_turnpikeStatus);
                PublicAvaliableReferences.UpdateAllLights(_turnpikeStatus);

                Thread.Sleep(THREAD_TICK);
            } while (true);
        }
    }
}
