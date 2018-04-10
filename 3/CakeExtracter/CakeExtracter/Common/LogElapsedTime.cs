using System;
using System.Diagnostics;

namespace CakeExtracter.Common
{
    public sealed class LogElapsedTime : IDisposable
    {
        private readonly Stopwatch stopWatch;
        private string _message;

        public LogElapsedTime(string message = null)
        {
            _message = message;
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        public void Dispose()
        {
            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                               ts.Hours, ts.Minutes, ts.Seconds,
                                               ts.Milliseconds / 10);

            string extra = String.IsNullOrWhiteSpace(_message) ? "" : " " + _message;

            Logger.Info("Elapsed Time: {0}{1}", elapsedTime, extra);
        }
    }
}