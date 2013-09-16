using System;

namespace Bottles.Services
{
    /// <summary>
    /// Strictly used within the main Program
    /// </summary>
    public class BottleServiceRuntime
    {
        private readonly Lazy<IApplicationLoader> _runner;
        private IDisposable _shutdown;

        public BottleServiceRuntime()
        {
            _runner = new Lazy<IApplicationLoader>(bootstrap);
        }

        private IApplicationLoader Runner
        {
            get { return _runner.Value; }
        }

        private IApplicationLoader bootstrap()
        {
            return BottleServiceApplication.FindLoader(null);
        }

        public void Start()
        {
            _shutdown = Runner.Load();
        }

        public void Stop()
        {
            _shutdown.Dispose();
        }
    }
}