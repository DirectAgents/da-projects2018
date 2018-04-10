using System.Collections.Generic;
using System.Reflection;

namespace CakeExtracter.Common
{
    public abstract class ConsoleCommand : ManyConsole.ConsoleCommand
    {
        private List<ConsoleCommand> commandsToRunBeforeThisCommand = new List<ConsoleCommand>();

        protected void RunBefore(ConsoleCommand consoleCommand)
        {
            this.commandsToRunBeforeThisCommand.Add(consoleCommand);
        }

        public int Run()
        {
            return this.Run(null);
        }

        public override int Run(string[] remainingArguments)
        {
            string commandName = this.GetType().Name;
            if (this.commandsToRunBeforeThisCommand.Count > 0)
            {
                Logger.Info("{0} has prerequisites..", commandName);
                foreach (var consoleCommand in this.commandsToRunBeforeThisCommand)
                {
                    consoleCommand.Run(null);
                }
                Logger.Info("Completed prerequisites for {0}", commandName);
            }

            Logger.Info("Executing command: {0}", commandName);

            using (new LogElapsedTime("for " + commandName))
            {
                var retCode = Execute(remainingArguments);
                return retCode;
            }
        }

        //Note: I believe this is only called by the LineCommander before a command is run (because the Command object is not re-instantiated).
        //      When a Command is first instantiated, we rely on default property values (e.g. 0 for int).
        //TODO?: Call this from the Run() method - so it always sets the default properties
        public abstract void ResetProperties();

        public abstract int Execute(string[] remainingArguments);

        public virtual IEnumerable<PropertyInfo> GetArgumentProperties()
        {
            return this.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
        }

        public virtual bool TrySetProperty(string propertyName, object propertyValue)
        {
            var property = this.GetType().GetProperty(propertyName);
            if (property != null)
            {
                property.SetValue(this, propertyValue);
                return true;
            }
            return false;
        }


    }
}
