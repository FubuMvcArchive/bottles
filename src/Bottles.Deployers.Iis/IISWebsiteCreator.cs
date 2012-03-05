using System;
using Bottles.Diagnostics;
using FubuCore;
using Microsoft.Web.Administration;

namespace Bottles.Deployers.Iis
{
    public class IisWebsiteCreator
    {
        public void Create(Website website)
        {
            //currenly only IIS 7
            using (var iisManager = new ServerManager())
            {
                var pool = iisManager.CreateAppPool(website.AppPool);
                pool.ManagedRuntimeVersion = "v4.0";
                pool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                pool.Enable32BitAppOnWin64 = false;

                pool.ProcessModel.IdleTimeout = convertToTimeSpan(website);

                poolIdentity(website, pool);
                

                LogWriter.Current.Indent(() =>
                {
                    Site site = createSite(website, iisManager);

                    Application app = createApp(website, site);

                    //flush the changes so that we can now tweak them.
                    iisManager.CommitChanges();

                    app.DirectoryBrowsing(website.DirectoryBrowsing);

                    // TODO -- just take these out
                    //app.AnonAuthentication(website.AnonAuth);
                    //app.BasicAuthentication(website.BasicAuth);
                    //app.WindowsAuthentication(website.WindowsAuth);

                    iisManager.CommitChanges();

                    LogWriter.Current.Success("Success.");
                });
            }
        }

        private static TimeSpan convertToTimeSpan(Website website)
        {
            var timeout = website.IdleTimeOut;
            timeout = timeout == 0 ? 20 : timeout;
            return new TimeSpan(0, 0, timeout, 0);
        }

        private static void poolIdentity(Website website, ApplicationPool pool)
        {
            if (website.IdentityType.IsEmpty()) return;

            if (website.HasCredentials())
            {
                pool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
                pool.ProcessModel.UserName = website.Username;
                pool.ProcessModel.Password = website.Password;
                return;
            }

            var t = (ProcessModelIdentityType) Enum.Parse(typeof (ProcessModelIdentityType), website.IdentityType, true);
            pool.ProcessModel.IdentityType = t;
        }

        private Application createApp(Website website, Site site)
        {
            LogWriter.Current.Highlight("Trying to create a new virtual directory '{0}' at '{1}'".ToFormat(website.VDir, website.VDirPhysicalPath.ToFullPath()));
                                
            new FileSystem().CreateDirectory(website.VDirPhysicalPath.ToFullPath());
            var app = site.CreateApplication(website.VDir, website.VDirPhysicalPath.ToFullPath(), website.ForceApp);
            app.ApplicationPoolName = website.AppPool;
            return app;
        }

        private Site createSite(Website website, ServerManager iisManager)
        {
            LogWriter.Current.Highlight("Trying to create a new website '{0}' at '{1}', port '{2}'",
                website.WebsiteName,
                website.WebsitePhysicalPath.ToFullPath(),
                website.Port);

            new FileSystem().CreateDirectory(website.WebsitePhysicalPath.ToFullPath());
            return iisManager.CreateSite(website.WebsiteName, website.WebsitePhysicalPath.ToFullPath(),
                                         website.Port, website.ForceWebsite);
        }
    }
}