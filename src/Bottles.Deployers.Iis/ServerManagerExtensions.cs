using System.Configuration;
using System.IO;
using System.Linq;
using FubuCore;
using Microsoft.Web.Administration;
using ConfigurationSection = Microsoft.Web.Administration.ConfigurationSection;

namespace Bottles.Deployers.Iis
{
    public static class ServerManagerExtensions
    {
        private static IFileSystem _fileSystem = new FileSystem();

        public static Site CreateSite(this ServerManager iisManager, string name, string directory, int port, bool force)
        {
            //add a guard clause for any sites already listening on this port?
            
            if (force && iisManager.HasSite(name))
            {
                iisManager.Sites[name]
                    .Delete();
                iisManager.CommitChanges();
            }

            if (iisManager.HasSite(name))
            {
                return iisManager.Sites[name];
            }

            _fileSystem.CreateDirectory(directory);
            return iisManager.Sites.Add(name, directory, port);
        }

        public static bool HasSite(this ServerManager iisManager, string name)
        {
            return iisManager.Sites.Any(s => s.Name.Equals(name));
        }

        public static Application CreateApplication(this Site site, string vdir, string physicalPath, bool force)
        {
            vdir = fixVDir(vdir);

            if(force && site.HasApplication(vdir))
            {
                site.Applications[vdir].Delete();
                //do I need to commit?
            }

            if (site.HasApplication(vdir))
            {
                return site.Applications[vdir];
            }

            _fileSystem.CreateDirectory(physicalPath);
            return site.Applications.Add(vdir, physicalPath);

        }

        public static bool HasApplication(this Site site, string vdir)
        {
            vdir = fixVDir(vdir);

            return site.Applications.Any(a => a.Path.Equals(vdir));
        }

        public static ApplicationPool CreateAppPool(this ServerManager iisManager, string name)
        {
            if (!iisManager.ApplicationPools.Any(p => p.Name.Equals(name)))
            {
                return iisManager.ApplicationPools.Add(name);
            }

            return iisManager.ApplicationPools[name];
        }

        public static void MapAspNetToEverything(this Application app)
        {
            try
            {

                var webCfg = app.GetWebConfiguration();
                var handlers = webCfg.GetSection("system.webServer/handlers");
                var handlersCollection = handlers.GetCollection();
                if (handlersCollection.Any(h => h["name"].Equals("HungryHungryDotNetHippo")))
                    return;

                var addElement = handlersCollection.CreateElement("add");
                addElement["name"] = "HungryHungryDotNetHippo";
                addElement["path"] = "*";
                addElement["verb"] = "*";
                addElement["type"] = "System.Web.UI.PageHandlerFactory";

                handlersCollection.AddAt(0, addElement);
            }
            catch (IOException ex)
            {
                //this might be because handlers are locked
                throw new ConfigurationErrorsException("IIS may not allow delegation of handlers after Win7 SP1 - please allow delegation", ex);
            }
        }

        public static void DirectoryBrowsing(this Application app, Activation activation)
        {
            ConfigurationSection directoryBrowseSection = app.GetWebConfiguration().GetSection("system.webServer/directoryBrowse");
            var b = activation == Activation.Enable;
            directoryBrowseSection["enabled"] = b;
            directoryBrowseSection["showFlags"] = @"Date, Time, Size, Extension";
        }

        public static void AnonAuthentication(this Application app, Activation activation)
        {
            var config = app.GetWebConfiguration().GetSection("system.webServer/security/authentication/anonymousAuthentication");
            config["enabled"] = activation == Activation.Enable;
        }

        public static void BasicAuthentication(this Application app, Activation activation)
        {
            var config = app.GetWebConfiguration().GetSection("system.webServer/security/authentication/basicAuthentication");
            config["enabled"] = activation == Activation.Enable;
        }

        public static void WindowsAuthentication(this Application app, Activation activation)
        {
            var config = app.GetWebConfiguration().GetSection("system.webServer/security/authentication/windowsAuthentication");
            config["enabled"] = activation == Activation.Enable;
        }
        static string fixVDir(string vdir)
        {
            if (vdir[0] != '/')
            {
                vdir = '/' + vdir;
            }
            return vdir;
        }
    }
}