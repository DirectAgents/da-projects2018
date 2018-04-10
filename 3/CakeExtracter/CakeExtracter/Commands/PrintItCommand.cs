using System.ComponentModel.Composition;
using System.Threading;
using CakeExtracter.Common;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class PrintIt : ConsoleCommand
    {
        public int Pause { get; set; }
        public int Iterations { get; set; }
        public string Message { get; set; }

        public override void ResetProperties()
        {
            Pause = 0;
            Iterations = 1;
            Message = null;
        }

        public PrintIt()
        {
            ResetProperties();
            IsCommand("printIt", "fake it");
            HasOption("t|Pause=", "ms to pause between prints (default is 0)", c => this.Pause = int.Parse(c));
            HasOption("i|Iterations=", "number times to execute (default is 1)", c => this.Iterations = int.Parse(c));
            HasOption("m|Message=", "message to print", c => this.Message = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            for (int i = 0; i < this.Iterations; i++)
            {
                System.Console.WriteLine(this.Message);
                Thread.Sleep(this.Pause);
            }
            return 0;
        }

    }

    [Export(typeof(ConsoleCommand))]
    public class PrintIt2 : ConsoleCommand
    {
        public int Pause { get; set; }
        public int Iterations { get; set; }
        public string Message { get; set; }

        public override void ResetProperties()
        {
            Pause = 0;
            Iterations = 1;
            Message = null;
        }

        public PrintIt2()
        {
            ResetProperties();
            RunBefore(new PrintIt() { Message = "dummy prerequisite" });

            IsCommand("printIt2", "fake it");
            HasOption("t|Pause=", "ms to pause between prints (default is 0)", c => this.Pause = int.Parse(c));
            HasOption("i|Iterations=", "number times to execute (default is 1)", c => this.Iterations = int.Parse(c));
            HasOption("m|Message=", "message to print", c => this.Message = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            for (int i = 0; i < this.Iterations; i++)
            {
                System.Console.WriteLine(this.Message);
                Thread.Sleep(this.Pause);
            }
            return 0;
        }

    }
}
