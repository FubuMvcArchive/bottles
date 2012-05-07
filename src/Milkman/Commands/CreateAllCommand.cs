using System.ComponentModel;
using System.IO;
using System.Linq;
using Bottles.Creation;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class CreateAllInput
    {
        public CreateAllInput()
        {
            TargetFlag = "Debug";
            DirectoryFlag = ".".ToFullPath();
        }

        [Description("Overrides the top level directory to begin searching for package manifests")]
        [FlagAlias("directory", 'd')]
        public string DirectoryFlag { get; set; }

        [Description("Overrides the deployment directory ~/deployment")]
        [FlagAlias("deployment", 'y')]
        public string DeploymentFlag { get; set; }

        [Description("Includes any matching .pdb files for the package assemblies")]
        [FlagAlias("pdb", 'b')]
        public bool PdbFlag { get; set; }

        [Description("Overrides the compilation target.  The default is Debug")]
        public string TargetFlag { get; set; }

        [Description("Directs the command to remove all bottle files before creating new files.  Can be destructive")]
        public bool CleanFlag { get; set; }
    }

    [CommandDescription("Creates all the packages for the directories / manifests listed in the bottles.manifest file and puts the new packages into the deployment/bottles directory", Name="create-all")]
    public class CreateAllCommand : FubuCommand<CreateAllInput>
    {
        public override bool Execute(CreateAllInput input)
        {
            return Execute(new FileSystem(), input);
        }

        public bool Execute(IFileSystem system, CreateAllInput input)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);

            var i =  new Bottles.Commands.CreateAllInput
            {
                DirectoryFlag = input.DirectoryFlag,
                DeploymentFlag = input.DeploymentFlag,
                PdbFlag = input.PdbFlag,
                TargetFlag = input.TargetFlag,
                CleanFlag = input.CleanFlag,
                OutputFlag = settings.BottlesDirectory
            };
            return new Bottles.Commands.CreateAllCommand().Execute(i);
        }
    }
}