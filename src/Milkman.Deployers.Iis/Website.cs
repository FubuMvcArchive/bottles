using Bottles.Deployment;
using Bottles.Deployment.Configuration;
using FubuCore;

namespace Bottles.Deployers.Iis
{
    public class Website : IDirective
    {
        public static readonly string WEBSITE_NAME = "website-name";
        public static readonly string VIRTUAL_DIR = "virtual-dir";
        public static readonly string APP_POOL = "app-pool";
        

        public Website()
        {
            Port = 80;
            DirectoryBrowsing = Activation.Disable;
            AnonAuth = Activation.Enable;
            BasicAuth = Activation.Disable;
            WindowsAuth = Activation.Disable;

            WebsitePhysicalPath = FileSystem.Combine(EnvironmentSettings.ROOT.ToSubstitution(), "web");
            VDir = VIRTUAL_DIR.ToSubstitution();
            VDirPhysicalPath = FileSystem.Combine(WebsitePhysicalPath, VDir);

            WebsiteName = WEBSITE_NAME.ToSubstitution();


            AppPool = APP_POOL.ToSubstitution();

            ForceWebsite = false;
            ForceApp = false;
        }

        /// <summary>
        /// IIS website name
        /// </summary>
        public string WebsiteName { get; set; }
        
        /// <summary>
        /// Path on disk to the Website
        /// </summary>
        public string WebsitePhysicalPath { get; set; }

        /// <summary>
        /// Application virtual directory in IIS
        /// </summary>
        public string VDir { get; set; }

        /// <summary>
        /// Physical path to virtual directory
        /// </summary>
        public string VDirPhysicalPath { get; set; }

        /// <summary>
        /// Which port to listen on
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The application pool to use
        /// </summary>
        public string AppPool { get; set; }

        /// <summary>
        /// The idle timeout in minutes
        /// </summary>
        public int IdleTimeOut;

        /// <summary>
        /// What process model do you want to use for the app pool
        /// </summary>
        public string IdentityType { get; set; }

        //credentials
        public string Username { get; set; }
        public string Password { get; set; }
        public bool HasCredentials()
        {
            return !string.IsNullOrEmpty(Username) && IdentityType.ToLower() == "specificUser";
        }

        //iis options
        public Activation DirectoryBrowsing { get; set; }
        public Activation AnonAuth { get; set; }
        public Activation BasicAuth { get; set; }
        public Activation WindowsAuth { get; set; }

        public bool ForceWebsite { get; set; }
        public bool ForceApp { get; set; }

        //what could be common options?
        // clean deletes the target directory before deploying.
        public bool Clean { get; set; }

        public override string ToString()
        {
            return string.Format("Website: {0} VDir: {1}, VDirPhysicalPath: {2}", WebsiteName, VDir, VDirPhysicalPath);
        }
    }
}