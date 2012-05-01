using System;
using System.ComponentModel;
using System.Threading;
using Bottles.Commands;
using Bottles.Diagnostics;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class InitializeInput
    {
        private readonly Lazy<DeploymentSettings> _settings;

        public InitializeInput()
        {
            _settings = new Lazy<DeploymentSettings>(() => DeploymentSettings.ForDirectory(DeploymentFlag));
        }

        public InitializeInput(DeploymentSettings settings)
        {
            _settings = new Lazy<DeploymentSettings>(() => settings);
        }

        [Description("Physical folder (or valid alias) of the application")]
        public string DeploymentFlag { get; set; }

        [FlagAlias("force", 'f')]
        public bool ForceFlag { get; set; }

        public DeploymentSettings Settings
        {
            get { return _settings.Value; }
        }
    }

    [CommandDescription("Seeds the /deployment folder structure underneath the root directory of a codebase", Name = "init")]
    public class InitializeCommand : FubuCommand<InitializeInput>
    {
        public static readonly string DIRECTORY_ALREADY_EXISTS =
            "Directory {0} already exists. Use the -f flag to overwrite the existing structure";

        public static readonly string DELETING_EXISTING_DIRECTORY = "Deleting existing deployment directory at {0}";

        public override bool Execute(InitializeInput input)
        {
            if (input.DeploymentFlag != null) input.DeploymentFlag = new AliasService().GetFolderForAlias(input.DeploymentFlag);

            return Initialize(input, new FileSystem());
        }

        public bool Initialize(InitializeInput input, IFileSystem fileSystem)
        {
            var deploymentDirectory = input.Settings.DeploymentDirectory;
            LogWriter.Current.Trace("Trying to initialize Bottles deployment folders at {0}", deploymentDirectory);

            if (fileSystem.DirectoryExists(deploymentDirectory))
            {
                if (input.ForceFlag)
                {
                    LogWriter.Current.Trace(DELETING_EXISTING_DIRECTORY, deploymentDirectory);
                    fileSystem.CleanDirectory(deploymentDirectory);
                    fileSystem.DeleteDirectory(deploymentDirectory);
                    Thread.Sleep(10); //file system is async
                }
                else
                {
                    LogWriter.Current.Trace(DIRECTORY_ALREADY_EXISTS, deploymentDirectory);
                    return false;
                }
            }

            createDirectory(fileSystem, deploymentDirectory);

            createDirectory(fileSystem, input.Settings.BottlesDirectory);
            createDirectory(fileSystem, input.Settings.RecipesDirectory);
            createDirectory(fileSystem, input.Settings.ProfilesDirectory);
            createDirectory(fileSystem, input.Settings.DeployersDirectory);

            return true;
        }

        private static void createDirectory(IFileSystem system, params string[] pathParts)
        {
            var directory = FileSystem.Combine(pathParts);

            LogWriter.Current.Trace("Creating directory " + directory);
            system.CreateDirectory(directory);
        }
    }
}