using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Mef;

namespace CakeExtracter
{
    class Program
    {
        private readonly Composer<Program> composer;

        [ImportMany]
        private IEnumerable<IBootstrapper> bootstrappers;

        [ImportMany]
        private IEnumerable<ConsoleCommand> Commands;

        private Program()
        {
            composer = new Composer<Program>(this);
            composer.Compose();

            bootstrappers.ToList().ForEach(c => c.Run()); 
        }

        private int Run(string[] args)
        {
            return ManyConsole.ConsoleCommandDispatcher.DispatchCommand(Commands, args, Console.Out);
        }

        // --- static methods ---
        [STAThread]
        public static int Main(string[] args)
        {
            var program = new Program();
            var result = program.Run(args);
            return result;
        }
    }
}
