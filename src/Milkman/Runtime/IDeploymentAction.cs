using Bottles.Diagnostics;

namespace Bottles.Deployment.Runtime
{
    public interface IDeploymentAction<T> where T : IDirective
    {
        void Execute(T directive, HostManifest host, IBottleLog log);
        string GetDescription(T directive);
    }
}