using System;
using System.Collections.Generic;
using System.Linq;

namespace Bottles.Services
{
	public class BottleServiceRuntime
	{
		private readonly Lazy<BottleServiceRunner> _runner;

		public BottleServiceRuntime()
		{
			_runner = new Lazy<BottleServiceRunner>(bootstrap);
		}

		private BottleServiceRunner Runner { get { return _runner.Value; } }

		private BottleServiceRunner bootstrap()
		{
			var application = new BottleServiceApplication();
			var runner = application.Bootstrap();

			if (!runner.Services.Any())
			{
				throw new ApplicationException("No services were detected.  Shutting down.");
			}

			runner.Services.Each(x => Console.WriteLine("Started " + x));

			return runner;
		}

		public void Start()
		{
			Runner.Start();
		}

		public void Stop()
		{
			Runner.Stop();
		}
	}
}