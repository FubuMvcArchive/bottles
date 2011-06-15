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

                if (website.HasCredentials())
                {
                    pool.ProcessModel.UserName = website.Username;
                    pool.ProcessModel.Password = website.Password;
                }

                LogWriter.Indent(() =>
                {
                    LogWriter.Highlight("Trying to create a new website at {0}, port {1}",
                                        website.WebsitePhysicalPath.ToFullPath(), website.Port);
                    var site = iisManager.CreateSite(website.WebsiteName, website.WebsitePhysicalPath.ToFullPath(),
                                                     website.Port);

                    LogWriter.Highlight("Trying to create a new virtual directory at " +
                                        website.VDirPhysicalPath.ToFullPath());
                    var app = site.CreateApplication(website.VDir, website.VDirPhysicalPath.ToFullPath());
                    app.ApplicationPoolName = website.AppPool;

                    //flush the changes so that we can now tweak them.
                    iisManager.CommitChanges();

                    app.DirectoryBrowsing(website.DirectoryBrowsing);

                    // TODO -- just take these out
                    //app.AnonAuthentication(website.AnonAuth);
                    //app.BasicAuthentication(website.BasicAuth);
                    //app.WindowsAuthentication(website.WindowsAuth);

                    iisManager.CommitChanges();

                    LogWriter.Success("Success.");
                });
            }
        }
    }
}