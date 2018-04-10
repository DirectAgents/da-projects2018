using System.Collections.Generic;

namespace CakeExtracter.Etl
{
    public interface ILoader<T>
    {
        int Load(List<T> items);
    }
}
