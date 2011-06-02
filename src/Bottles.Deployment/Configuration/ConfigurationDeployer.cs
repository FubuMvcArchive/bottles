using System;
using System.IO;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;
using System.Collections.Generic;
using FubuCore.Configuration;

namespace Bottles.Deployment.Configuration
{
    public class ConfigurationDirectory : IDirective
    {
        public ConfigurationDirectory()
        {
            ConfigDirectory = "config";
        }

        public string ConfigDirectory { get; set;}
    }

    public class ConfigurationDeployer : IDeployer<ConfigurationDirectory>
    {
        private readonly DeploymentSettings _deploymentSettings;
        private readonly IFileSystem _fileSystem;
        private readonly IBottleRepository _repository;

        public ConfigurationDeployer(DeploymentSettings deploymentSettings, IFileSystem fileSystem, IBottleRepository repository)
        {
            _deploymentSettings = deploymentSettings;
            _fileSystem = fileSystem;
            _repository = repository;
        }


        public void Execute(ConfigurationDirectory directive, HostManifest host, IPackageLog log)
        {
            // copy the environment settings there
            // explode the bottles out to there
            // log each file to there

            var configDirectory = directive.ConfigDirectory.CombineToPath(_deploymentSettings.TargetDirectory);
            _fileSystem.CreateDirectory(configDirectory);

            writeEnvironmentFile(configDirectory, log);
            
            if (_deploymentSettings.Profile != null)
            {
                var profileFile = configDirectory.AppendPath("profile.config");
                XmlSettingsParser.Write(_deploymentSettings.Profile.Data, profileFile);
            }


            // TODO -- diagnostics?
            host.BottleReferences.Each(x =>
            {
                var request = new BottleExplosionRequest(new PackageLog())
                {
                    BottleDirectory = BottleFiles.ConfigFolder,
                    BottleName = x.Name,
                    DestinationDirectory = configDirectory
                };

                _repository.ExplodeFiles(request);
            });
        }

        private void writeEnvironmentFile(string configDirectory, IPackageLog log)
        {
            var environmentFile = configDirectory.AppendPath("environment.config");
            log.Trace("Writing the environment settings to " + environmentFile);
            XmlSettingsParser.Write(_deploymentSettings.Environment.Data, environmentFile);
        }
    }
}