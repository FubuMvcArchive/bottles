namespace Bottles.Host.Packaging
{
    /// <summary>
    /// The clients entry point into the process.
    /// </summary>
    public interface IBottleAwareService : IBootstrapper
    {
        void Stop();
    }
}