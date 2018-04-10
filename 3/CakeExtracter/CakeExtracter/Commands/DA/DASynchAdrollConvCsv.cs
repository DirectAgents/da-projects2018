using System.ComponentModel.Composition;
using System.IO;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.LoadersDA;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchAdrollConvCsv : ConsoleCommand
    {
        public static int RunStatic(int accountId, StreamReader streamReader)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchAdrollConvCsv
            {
                AccountId = accountId,
                StreamReader = streamReader
            };
            int result = cmd.Run();
            return result;
        }

        public StreamReader StreamReader { get; set; }

        public string CsvFilePath { get; set; }
        //public int AdRollProfileId { get; set; }
        public int AccountId { get; set; }

        public override void ResetProperties()
        {
            CsvFilePath = null;
            //AdRollProfileId = -1;
            AccountId = -1;
        }

        //TODO: account id / advertiseable id

        public DASynchAdrollConvCsv()
        {
            IsCommand("daSynchAdrollConvCsv", "synch Conversions from AdRoll CSV Report");
            HasRequiredOption("f|csvFilePath=", "CSV filepath", c => CsvFilePath = c);
            //HasRequiredOption<int>("p|adrollProfileId=", "AdRollProfile id", c => AdRollProfileId = c);
            HasRequiredOption<int>("a|accountId=", "ExtAccount id", c => AccountId = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var extracter = new AdrollConvCsvExtracter(CsvFilePath, StreamReader);
            var loader = new AdrollConvLoader(AccountId);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            return 0;
        }
    }
}
