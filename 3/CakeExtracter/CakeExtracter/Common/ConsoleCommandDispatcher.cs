namespace CakeExtracter.Common
{
    public static class ConsoleCommandDispatcher
    {
        public static int DispatchCommand(System.Collections.Generic.IEnumerable<ConsoleCommand> commands, string[] arguments, System.IO.TextWriter consoleOut, bool skipExeInExpectedUsage = false)
        {
            return ManyConsole.ConsoleCommandDispatcher.DispatchCommand(commands, arguments, consoleOut, skipExeInExpectedUsage);
        }
    }
}
