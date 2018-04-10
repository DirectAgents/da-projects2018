using System;
using System.Linq;
using System.Runtime.Remoting;
using CakeExtracter.Common;
using Quartz;

namespace CakeExtracter.Commands
{
    public class ScheduledJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            // Per documentation, quartz.net job execution should be in a try/catch block
            try
            {
                // Get the job data map from the context
                var jobDataMap = context.JobDetail.JobDataMap;

                // Get the type name and assembly name from the job data map
                var typeName = (string)jobDataMap["typeName"];
                var assemblyName = (string)jobDataMap["assemblyName"];

                // Use Activator to create the target command by name
                ObjectHandle handle = Activator.CreateInstance(assemblyName, typeName);
                var consoleCommand = (ConsoleCommand)handle.Unwrap();

                // Copy property values from the job data map to the target command
                var propertyPrefix = "property.";
                foreach (var item in jobDataMap.Where(c => c.Key.StartsWith(propertyPrefix))
                                               .Select(c => new
                                               {
                                                   PropertyName = c.Key.Substring(propertyPrefix.Length),
                                                   PropertyValue = c.Value
                                               }))
                {
                    consoleCommand.GetType().GetProperty(item.PropertyName).SetValue(consoleCommand, item.PropertyValue);
                }

                Logger.Info("ScheduledJob is executing command: {0}", consoleCommand.Command);

                // Execute the target command
                consoleCommand.Run(null);
            }
            catch (Exception ex) // TODO: consult quartz.net docs for how to properly deal with exceptions
            {
                Logger.Error(ex);
                throw;
            }
        }
    }
}
