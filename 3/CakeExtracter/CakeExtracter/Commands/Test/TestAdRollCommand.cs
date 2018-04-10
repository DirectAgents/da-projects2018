using System;
using System.ComponentModel.Composition;
using AdRoll;
using CakeExtracter.Common;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestAdRollCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public TestAdRollCommand()
        {
            IsCommand("testAdRoll");
        }

        public override int Execute(string[] remainingArguments)
        {
            var arUtility = new AdRollUtility(m => Logger.Info(m), m => Logger.Warn(m));
            var advertisableId = "HUULCFSLCVEBHPBXVDCIEN"; // AHS
            var date = DateTime.Today.AddDays(-1);
            var adSummaries = arUtility.AdDailySummaries(date, advertisableId);

            return 0;
        }
    }
}
