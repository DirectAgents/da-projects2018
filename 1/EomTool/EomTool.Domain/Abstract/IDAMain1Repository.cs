using EomTool.Domain.Entities;
using System;
using System.Linq;

namespace EomTool.Domain.Abstract
{
    public interface IDAMain1Repository : IDisposable
    {
        void SaveChanges();

        IQueryable<DADatabase> DADatabases { get; }

        IQueryable<PublisherNote> PublisherNotes { get; }
        IQueryable<PublisherNote> PublisherNotesForPublisher(string pubName);
        void AddPublisherNote(string pubName, string note, string identity);

        string GetSettingValue(string name);
        int? GetSettingIntValue(string name);
        decimal? GetSettingDecimalValue(string name);
        void SaveSetting(string name, string value);
        void SaveSetting(string name, int? value);
        void SaveSetting(string name, decimal? value);
    }
}
