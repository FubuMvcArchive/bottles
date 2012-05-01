namespace Bottles
{
    /// <summary>
    /// This reads in a directory, finds the package manifest and 
    /// returns an IPackageInfo.
    /// 
    /// This should be safe to rename.
    /// 
    /// This is the default reader (also an assembly one would make sense)
    /// </summary>
    public interface IPackageManifestReader
    {
        IPackageInfo LoadFromFolder(string packageDirectory);
    }
}