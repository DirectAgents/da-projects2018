using System;
using System.Linq;
using System.Threading;

namespace CakeExtracter.Common
{
    public static class RetryUtility
    {
        public static T Retry<T>(int maxAttempts, int pauseMillisecondsBetweenAttempts, Type[] retryWhenCaught, Func<T> action)
        {
            return Retry(maxAttempts, pauseMillisecondsBetweenAttempts, 0, retryWhenCaught, action);
        }

        private static T Retry<T>(int maxAttempts, int pauseMillisecondsBetweenAttempts, int attemptNumber, Type[] retryWhenCaught, Func<T> action)
        {
            try
            {
                Logger.Info("Attempt #{0}..", attemptNumber);
                return action();
            }
            catch (Exception ex)
            {
                if (retryWhenCaught.Any(c => c.IsAssignableFrom(ex.GetType())))
                {
                    Logger.Warn("Caught {0}: {1}", ex.GetType().Name, ex.Message + "\n----------\n" + ex.StackTrace);
                    if (attemptNumber < maxAttempts)
                    {
                        Logger.Info("Pausing for {0} ms before next attempt..", pauseMillisecondsBetweenAttempts);
                        Thread.Sleep(pauseMillisecondsBetweenAttempts);
                        return Retry(maxAttempts, pauseMillisecondsBetweenAttempts, attemptNumber + 1, retryWhenCaught, action);
                    }
                }
                throw;
            }
        }
    }
}