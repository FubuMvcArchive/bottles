using System.Collections.Generic;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Deployers.Configuration
{
    public class ConfigInitializer : IInitializer<CentralConfig>
    {
        private readonly IBottleRepository _repository;
        private readonly DeploymentSettings _settings;
        private readonly IFileSystem _fileSystem;
        private readonly IConfigurationWriter _writer;

        public ConfigInitializer(IBottleRepository repository, DeploymentSettings settings, IFileSystem fileSystem, IConfigurationWriter writer)
        {
            _repository = repository;
            _settings = settings;
            _fileSystem = fileSystem;
            _writer = writer;
        }

        public void Execute(CentralConfig directive, HostManifest host, IPackageLog log)
        {
            log.Trace("Creating folder " + directive.Directory);
            _fileSystem.CreateDirectory(directive.Directory);


            host.BottleReferences.Each(r =>
            {
                log.Trace("Exploding bottle {0} to {1}", r.Name, directive.Directory);
                _repository.ExplodeTo(r.Name, directive.Directory);
            });

            if (_settings.Environment != null)
            {
                var filename = directive.Directory.AppendPath("EnvironmentSettings.config");
                _writer.Write(filename, _settings.Environment);
            }

            if (_settings.Profile != null)
            {
                var filename = directive.Directory.AppendPath("Profile.config");
                _writer.Write(filename, _settings.Profile);
            }
        }
    }
}