using System;

namespace CakeExtracter.Logging.Loggers
{
    public class ConsoleLogger : ILogger
    {
        public void Info(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Console.WriteLine("Warn: " + format, args);
        }

        public void Error(Exception exception)
        {
            Console.WriteLine("Exception: " + exception.Message);
        }

        public void Trace(string format, params object[] args)
        {
            Console.WriteLine("Trace: " + format, args);
        }
    }
}