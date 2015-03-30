using Bottles.Services;
using FubuCore;
using Topshelf;
using System;

namespace BottleServiceRunner
{
    internal static class Program
    {
        public static void Main(params string[] args)
        {
	        var settings = serviceConfiguration();

            HostFactory.Run(x => {
                x.SetServiceName(settings.Name);
                x.SetDisplayName(settings.DisplayName);
                x.SetDescription(settings.Description);

                if (Platform.IsUnix ())
                {
                    x.RunAsPrompt();
                } else
                {
                    x.RunAsLocalService();
                }

				if (settings.UseEventLog)
				{
					x.UseEventLog(settings);
				}

				x.Service<BottleServiceRuntime>(s =>
				{
                    s.ConstructUsing(name => new BottleServiceRuntime(settings));
                    s.WhenStarted(r => r.Start());
                    s.WhenStopped(r => r.Stop());
                    s.WhenPaused(r => r.Stop());
                    s.WhenContinued(r => r.Start());
                    s.WhenShutdown(r => r.Stop());
                });

                x.StartAutomatically();
            });
        }

		private static BottleServiceConfiguration serviceConfiguration()
		{
			var directory = BottlesServicePackageFacility.GetApplicationDirectory();
			var fileSystem = new FileSystem();

			return fileSystem.LoadFromFile<BottleServiceConfiguration>(directory, BottleServiceConfiguration.FILE);
		}
    }
}
