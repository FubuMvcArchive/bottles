using Bottles.Diagnostics;

namespace Bottles.Environment
{
    public interface IInstaller
    {
        void Install(IBottleLog log);
        void CheckEnvironment(IBottleLog log);
    }


    // Teardown of the environment?
}