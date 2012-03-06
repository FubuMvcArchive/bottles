using System;
using Bottles.Deployment;
using FubuCore.CommandLine;

namespace milk
{
    class Program
    {
        private static bool success;

        static int Main(string[] args)
        {
            try
            {
                var factory = new CommandFactory();
                factory.SetAppName("milk");
                factory.RegisterCommands(typeof(IFubuCommand).Assembly);
                factory.RegisterCommands(typeof(Recipe).Assembly);

                var executor = new CommandExecutor(factory);
                success = executor.Execute(args);
            }
            catch (CommandFailureException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleWriter.Write("ERROR: " + e.Message);
                Console.ResetColor();
                return 1;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleWriter.Write("ERROR: " + ex);
                Console.ResetColor();
                return 1;
            }
            return success ? 0 : 1;
        }
    }
}
