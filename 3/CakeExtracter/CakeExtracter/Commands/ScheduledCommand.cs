using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using CakeExtracter.Common;
using Quartz;

namespace CakeExtracter.Commands
{
    public class ScheduledCommand : ConsoleCommand
    {
        public int IntervalCount { get; set; }
        public char IntervalUnit { get; set; }
        public int StartHoursFromNow { get; set; }

        private IScheduler scheduler;
        private ConsoleCommand consoleCommandToExecute;

        public override void ResetProperties()
        {
            IntervalCount = 0;
            IntervalUnit = ' ';
            StartHoursFromNow = 0;

            consoleCommandToExecute.ResetProperties();
        }

        public ScheduledCommand(IScheduler scheduler, ConsoleCommand consoleCommandToExecute)
        {
            this.scheduler = scheduler;
            this.consoleCommandToExecute = consoleCommandToExecute;

            IsCommand(this.consoleCommandToExecute.Command, "scheduled command: " + this.consoleCommandToExecute.Command); // need this?
        }

        public override int Run(string[] remainingArguments)
        {
            return this.IntervalCount > 0
                            ? RunSchedule()
                            : this.consoleCommandToExecute.Run(remainingArguments);
        }

        public override int Execute(string[] remainingArguments)
        {
            throw new Exception("ScheduledCommand.Execute() should never be called.");
        }

        private int RunSchedule()
        {
            // Make sure this is a unique name, otherwise parallel jobs will not be scheduled
            string thisTypeName = this.GetType().Name + "_" + this.consoleCommandToExecute.GetType().FullName + "_" + Guid.NewGuid().ToString();

            var jobDetailIdentity = "Job_" + thisTypeName;

            var groupIdentity = "Group_" + thisTypeName;

            Logger.Info("Scheduling with group identity {0}..", groupIdentity);

            Logger.Info("Creating job detail with identity {0}..", jobDetailIdentity);

            var jobDetail = JobBuilder
                                .Create<ScheduledJob>()
                                .WithIdentity(jobDetailIdentity, groupIdentity)
                                .Build();

            var jobDataMap = jobDetail.JobDataMap;
            Type consoleCommandToExecuteType = this.consoleCommandToExecute.GetType();
            jobDataMap["typeName"] = consoleCommandToExecuteType.FullName;
            jobDataMap["assemblyName"] = consoleCommandToExecuteType.Assembly.FullName;
            foreach (var argumentProperty in this.consoleCommandToExecute.GetArgumentProperties())
            {
                jobDetail.JobDataMap["property." + argumentProperty.Name] = consoleCommandToExecuteType
                                                                                    .GetProperty(argumentProperty.Name)
                                                                                    .GetValue(this.consoleCommandToExecute);
            }

            var triggerIdentity = "Trigger_" + thisTypeName;

            Logger.Info("Creating trigger with identity {0}..", triggerIdentity);

            var triggerBuilder = TriggerBuilder
                                    .Create()
                                    .WithIdentity(triggerIdentity, groupIdentity)
                                    .WithSimpleSchedule(c => c.WithInterval(GetInterval()).RepeatForever());

            if (StartHoursFromNow > 0)
            {
                Logger.Info("Configuring trigger to start {0} hours from now..", StartHoursFromNow);

                triggerBuilder.StartAt(DateTimeOffset.UtcNow.AddHours(StartHoursFromNow));
            }
            else
            {
                Logger.Info("Configuring trigger to start now..");

                triggerBuilder.StartNow();
            }

            var trigger = triggerBuilder.Build();

            this.scheduler.ScheduleJob(jobDetail, trigger);

            Logger.Warn("This thread will now effectively sleep forever (log message every minute) so the scheduler can run..");

            SleepLoopForever();

            return 0;
        }

        private TimeSpan GetInterval()
        {
            TimeSpan result;
            switch (IntervalUnit)
            {
                case 'h':
                    result = TimeSpan.FromHours(IntervalCount);
                    break;
                case 'm':
                    result = TimeSpan.FromMinutes(IntervalCount);
                    break;
                case 's':
                    result = TimeSpan.FromSeconds(IntervalCount);
                    break;
                default:
                    throw new Exception("invalid interval unit");
            }
            return result;
        }

        private void SleepLoopForever()
        {
            var runningCommandTypeName = this.consoleCommandToExecute.GetType().Name;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                Logger.Info("Scheduled {0} running for {1}", runningCommandTypeName, stopwatch.Elapsed.ToString());
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        public override IEnumerable<PropertyInfo> GetArgumentProperties()
        {
            return this.consoleCommandToExecute.GetArgumentProperties().Concat(base.GetArgumentProperties());
        }

        public override bool TrySetProperty(string propertyName, object propertyValue)
        {
            return this.consoleCommandToExecute.TrySetProperty(propertyName, propertyValue)
                   ||
                   base.TrySetProperty(propertyName, propertyValue);
        }
    }
}
