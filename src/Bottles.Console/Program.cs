using System;
using Bottles.Exploding;
using FubuCore.CommandLine;

namespace Bottles
{
    internal class Program
    {
        private static bool success;

        private static int Main(string[] args)
        {
            try
            {
                var factory = new CommandFactory();
                factory.SetAppName("bottles");
                factory.RegisterCommands(typeof(IFubuCommand).Assembly);
                factory.RegisterCommands(typeof(BottleExploder).Assembly);

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
