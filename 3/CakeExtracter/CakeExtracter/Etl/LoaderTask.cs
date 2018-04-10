using System.Collections.Concurrent;
using System.Threading.Tasks;
using CakeExtracter.Common;

namespace CakeExtracter.Etl
{
    class LoaderTask<T>
    {
        public LoaderTask(ILoader<T> loader, int batchSize)
        {
            Loader = loader;
            Collection = new BlockingCollection<T>(5000);
        }

        public void Start()
        {
            Task = new Task<int>(() =>
            {
                var loadedCount = 0;
                foreach (var list in Collection.GetConsumingEnumerable().InBatches(BatchSize))
                {
                    loadedCount += Loader.Load(list);
                }
                return loadedCount;
            });
        }

        public Task<int> Task { get; set; }

        public BlockingCollection<T> Collection { get; set; }

        public ILoader<T> Loader { get; set; }

        public int BatchSize { get; set; }
    }
}
