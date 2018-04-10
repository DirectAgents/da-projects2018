using DirectAgents.Domain.Entities.Wiki;
using System.Collections.Generic;
using System.Diagnostics;

namespace DirectAgents.Domain.Abstract
{
    public delegate void LogEventHandler(object sender, TraceEventType severity, string messageFormat, params object[] formatArgs);

    public interface IAdmin
    {
        event LogEventHandler LogHandler;

        string Test();
        string Test2();

        void CreateDatabaseIfNotExists();
        //void ReCreateDatabase();

        IEnumerable<Campaign> CampaignsNotInCake();

        void LoadSummaries();
        void LoadCampaigns();
    }
}
