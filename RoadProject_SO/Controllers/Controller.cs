using System.Threading;

namespace RoadProject_SO.Controllers
{
    public abstract class Controller
    {
        public const int THREAD_TICK = 10;
        private readonly Thread thread;

        protected Controller()
        {
            thread = new Thread(new ThreadStart(this.RunThread));
            thread.IsBackground = true;
        }

        //Thread methods / properties
        public void Start() => thread.Start();
        public void Join() => thread.Join();
        public bool IsAlive => thread.IsAlive;
        public void Abort() => thread.Abort();


        // Override in base class
        protected abstract void RunThread();
    }
}
