using System;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Runtime
{

    public interface IDeploymentFileCopier
    {
        void CopyFile(Func<DeploymentSettings, string> pathSource);
    }

    public class DeploymentFileCopier : IDeploymentFileCopier
    {
        private readonly IFileSystem _system;
        private readonly DeploymentSettings _source;
        private readonly DeploymentSettings _destination;

        public DeploymentFileCopier(IFileSystem system, DeploymentSettings source, DeploymentSettings destination)
        {
            _system = system;
            _source = source;
            _destination = destination;
        }

        public void CopyFile(Func<DeploymentSettings, string> pathSource)
        {
            var source = pathSource(_source);
            var destination = pathSource(_destination);


            LogWriter.Current.Trace("Copying {0} to {1}", source, destination);
            _system.Copy(source, destination);
        }
    }
}