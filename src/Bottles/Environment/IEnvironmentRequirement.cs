using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Environment
{
    public interface IEnvironmentRequirement
    {
        string Describe();
        void Check(IPackageLog log);
    }

    public interface IEnvironmentRequirements
    {
        IEnumerable<IEnvironmentRequirement> Requirements();
    }

    /*
     * 
     * 1.) file should exist
     * 2.) should be able to read folder
     * 3.) should be able to write to folder
     * 4.) able to load type?
     * 5.) able to connect to db
     * 6.) able to call to a Url
     * 
     * 
     * 
     */
}