using Bottles.Deployment;

namespace Bottles.Deployers.Topshelf
{
    public class TopshelfService : IDirective
    {
        public string InstallLocation { get; set; }
        public string Bootstrapper { get; set; }

        //optional
        public string Username { get; set; }
        public string Password { get; set; }

        //optional
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return string.Format("InstallLocation: {0}, ServiceName: {1}", InstallLocation, ServiceName);
        }
    }
}