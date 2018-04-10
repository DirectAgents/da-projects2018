using System;

namespace EomTool.Domain.Abstract
{
    public interface IEomEntitiesConfig
    {
        string ConnectionString { get; }
        DateTime CurrentEomDate { get; set; }
        string CurrentEomDateString { get; }
        bool DebugMode { get; }
    }
}
