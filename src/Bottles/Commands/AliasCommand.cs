using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    public class AliasInput
    {
        [Description("The name of the alias")]
        public string Name { get; set; }

        [Description("The path to the actual folder")]
        public string Folder { get; set; }

        [Description("Removes the alias")]
        public bool RemoveFlag { get; set; }
    }


    [CommandDescription("Manage folder aliases")]
    public class AliasCommand : FubuCommand<AliasInput>
    {
        public AliasCommand()
        {
            Usage("List all the aliases for this solution folder").Arguments().ValidFlags();
            Usage("Creates a new alias for a folder").Arguments(x => x.Name, x => x.Folder).ValidFlags();
            Usage("Removes the alias").Arguments(x => x.Name).ValidFlags(x => x.RemoveFlag);
        }

        public override bool Execute(AliasInput input)
        {
            Execute(input, new FileSystem());
            return true;
        }

        public void Execute(AliasInput input, IFileSystem system)
        {
            var registry = system.LoadFromFile<AliasRegistry>(AliasRegistry.ALIAS_FILE);
            if (input.Name.IsEmpty())
            {
                writeAliases(registry);
                return;
            }

            if (input.RemoveFlag)
            {
                registry.RemoveAlias(input.Name);
                ConsoleWriter.Write("Alias {0} removed", input.Name);
            }
            else
            {
                registry.CreateAlias(input.Name, input.Folder);
                ConsoleWriter.Write("Alias {0} created for folder {1}", input.Name, input.Folder);
            }

            persist(system, registry);
        }

        private void writeAliases(AliasRegistry registry)
        {
            if (!registry.Aliases.Any())
            {
                ConsoleWriter.Write(" No aliases are registered");
                return;
            }

            var maximumLength = registry.Aliases.Select(x => x.Name.Length).Max();
            var format = "  {0," + maximumLength + "} -> {1}";

            ConsoleWriter.Line();
            ConsoleWriter.PrintHorizontalLine();
            ConsoleWriter.Write(" Aliases:");
            ConsoleWriter.PrintHorizontalLine();

            registry.Aliases.OrderBy(x => x.Name).Each(x => { ConsoleWriter.Write(format, x.Name, x.Folder); });

            ConsoleWriter.PrintHorizontalLine();
        }

        private void persist(IFileSystem system, AliasRegistry registry)
        {
            system.WriteObjectToFile(AliasRegistry.ALIAS_FILE, registry);
        }
    }

}