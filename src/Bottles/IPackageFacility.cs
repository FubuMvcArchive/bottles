using System;
using System.Collections.Generic;
using System.Reflection;
using Bottles.Diagnostics;

namespace Bottles
{
    /// <summary>
    /// The bottle environment configuration.
    /// </summary>
    public interface IPackageFacility
    {
        /// <summary>
        /// Adds an assembly directly to the list of assemblies loaded into the environment.
        /// </summary>
        /// <param name="assembly"></param>
        void Assembly(Assembly assembly);

        /// <summary>
        /// Adds a bootstrapper that is to be run.
        /// There can be multiple. They return activators.
        /// </summary>
        /// <param name="bootstrapper"></param>
        void Bootstrapper(IBootstrapper bootstrapper);

        /// <summary>
        /// Lambda bootstrapper for convience
        /// </summary>
        /// <param name="lambda"></param>
        void Bootstrap(Func<IPackageLog, IEnumerable<IActivator>> lambda);

        /// <summary>
        /// Register an additional loader type.
        /// - determines how your framework (FubuMVC, Topshelf) interpret bottles/packages
        /// - controls the format of the package (assembly, zip file)
        /// </summary>
        /// <param name="loader"></param>
        void Loader(IPackageLoader loader);

        /// <summary>
        /// Adds another standard package facility configuration
        /// </summary>
        /// <param name="facility"></param>
        void Facility(IPackageFacility facility);

        /// <summary>
        /// Adds a one off activator. Great for if your framework needs to do a few
        /// things at start up.
        /// </summary>
        /// <param name="activator"></param>
        void Activator(IActivator activator);
    }

}