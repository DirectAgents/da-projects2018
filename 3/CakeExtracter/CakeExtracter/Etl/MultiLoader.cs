using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CakeExtracter.Etl
{
    public class MultiLoader<T>
    {
        private Extracter<T> extracter;
        private ILoader<T>[] destinations;

        public MultiLoader(ILoader<T>[] destinations)
        {
            this.destinations = destinations;
            BatchSize = 100;
        }

        public Thread Start(Extracter<T> source)
        {
            extracter = source;
            var thread = new Thread(DoLoad);
            thread.Start();
            return thread;
        }

        private void DoLoad()
        {
            var workers = new List<LoaderTask<T>>();

            // Create workers
            foreach (var loader in destinations)
            {
                workers.Add(new LoaderTask<T>(loader, BatchSize));
            }

            // Start workers
            foreach (var worker in workers)
            {
                worker.Start();
            }

            // Wait for all workers to complete
            Task.WaitAll(workers.Select(c => c.Task).ToArray());

            // Check item counts
            var counts = workers.Select(c => c.Task.Result);
            if (counts.Any(c => c != extracter.Added))
            {
                var ex = new Exception(string.Format("Unmatched counts: expecting {0}", extracter.Added));
                Logger.Error(ex);
                throw ex;
            }
        }

        public int BatchSize { get; set; }
    }
}
