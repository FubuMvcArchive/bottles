using System;
using System.ComponentModel;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    public class InitPakInput
    {
        [Description("The physical path to the new package")]
        public string Path { get; set; }

        [Description("The name of the new package")]
        [FlagAlias("name", 'n')]
        public string Name { get; set; }

        [Description("What role should this pak play - Options: module (default), binaries, config, application")]
        public string RoleFlag { get; set; }

        [Description("Creates a folder alias for the package folder.  Equivalent to fubu alias <folder> <alias>")]
        public string AliasFlag { get; set; }

        [Description("Opens the package manifest file in notepad")]
        public bool OpenFlag { get; set; }

        [Description("There is no web content to include")]
        [FlagAlias("noweb", 'w')]
        public bool NoWebContentFlag { get; set; }

        [Description("Force the command to overwrite any existing manifest file if using the -create flag")]
        [FlagAlias("force", 'f')]
        public bool ForceFlag { get; set; }

    }

    [CommandDescription("Initialize a package manifest", Name = "init-pak")]
    public class InitPakCommand : FubuCommand<InitPakInput>
    {
        public override bool Execute(InitPakInput input)
        {
            ConsoleWriter.Write(ConsoleColor.Red, "This command is obsolete, use 'init' instead");

            var input2 = new InitInput()
                         {
                             AliasFlag = input.AliasFlag,
                             ForceFlag = input.ForceFlag,
                             Name = input.Name,
                             NoWebContentFlag = input.NoWebContentFlag,
                             OpenFlag = input.OpenFlag,
                             Path = input.Path,
                             RoleFlag = input.RoleFlag,
                         };

            return new InitCommand().Execute(input2);
        }
    }
}