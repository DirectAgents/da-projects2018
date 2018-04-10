using System.Linq;
using EomTool.Domain.Entities;

namespace EomTool.Domain.Abstract
{
    public interface IBatchRepository
    {
        IQueryable<Batch> BatchesByBatchIds(int[] batchIds, bool includeNotes);
    }
}
