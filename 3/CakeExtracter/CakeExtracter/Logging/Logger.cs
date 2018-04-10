using System;
using CakeExtracter.Logging.Loggers;

namespace CakeExtracter
{
    public static class Logger
    {
        public static ILogger Instance = new NullLogger();

        public static void Info(string format, params object[] args)
        {
            Instance.Info(format, args);
        }

        public static void Warn(string format, params object[] args)
        {
            Instance.Warn(format, args); 
        }

        public static void Error(Exception exception)
        {
            Instance.Error(exception);
        }
    }
}
