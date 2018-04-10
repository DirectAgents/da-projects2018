using System;
using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Domain.DTO
{
    public class TDStatsGauge
    {
        public Platform Platform { get; set; }
        public ExtAccount ExtAccount { get; set; }

        public IEnumerable<TDStatsGauge> Children { get; set; }

        //TODO: replace with IStatsRanges?

        public TDStatRange Daily;
        public TDStatRange Strategy;
        public TDStatRange Creative;
        public TDStatRange AdSet;
        public TDStatRange Site;
        public TDStatRange Conv;
        public TDStatRange Action;

        //public void Reset()
        //{
        //    Daily.Earliest = null;
        //    Daily.Latest = null;
        //    Strategy.Earliest = null;
        //    Strategy.Latest = null;
        //    Creative.Earliest = null;
        //    Creative.Latest = null;
        //    AdSet.Earliest = null;
        //    AdSet.Latest = null;
        //    Site.Earliest = null;
        //    Site.Latest = null;
        //    Conv.Earliest = null;
        //    Conv.Latest = null;
        //    Action.Earliest = null;
        //    Action.Latest = null;
        //}
        public void SetFrom(IEnumerable<TDStatsGauge> gauges)
        {
            this.Daily.Earliest = gauges.Min(x => x.Daily.Earliest);
            this.Daily.Latest = gauges.Max(x => x.Daily.Latest);
            this.Strategy.Earliest = gauges.Min(x => x.Strategy.Earliest);
            this.Strategy.Latest = gauges.Max(x => x.Strategy.Latest);
            this.Creative.Earliest = gauges.Min(x => x.Creative.Earliest);
            this.Creative.Latest = gauges.Max(x => x.Creative.Latest);
            this.AdSet.Earliest = gauges.Min(x => x.AdSet.Earliest);
            this.AdSet.Latest = gauges.Max(x => x.AdSet.Latest);
            this.Site.Earliest = gauges.Min(x => x.Site.Earliest);
            this.Site.Latest = gauges.Max(x => x.Site.Latest);
            this.Conv.Earliest = gauges.Min(x => x.Conv.Earliest);
            this.Conv.Latest = gauges.Max(x => x.Conv.Latest);
            this.Action.Earliest = gauges.Min(x => x.Action.Earliest);
            this.Action.Latest = gauges.Max(x => x.Action.Latest);
        }
    }
    public struct TDStatRange
    {
        public DateTime? Earliest { get; set; }
        public DateTime? Latest { get; set; }

        public string EarliestDisp
        {
            get { return Earliest.HasValue ? Earliest.Value.ToShortDateString() : null; }
        }
        public string LatestDisp
        {
            get { return Latest.HasValue ? Latest.Value.ToShortDateString() : null; }
        }
    }

    public interface IStatsRange
    {
        DateTime? Earliest { get; }
        DateTime? Latest { get; }
    }

    public class SimpleStatsRange : IStatsRange
    {
        public DateTime? Earliest { get; set; }
        public DateTime? Latest { get; set; }

        public void UpdateWith(IStatsRange statsRange)
        {
            SetIfEarlier(statsRange.Earliest);
            SetIfLater(statsRange.Latest);
        }
        public void SetIfEarlier(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return;
            if (!this.Earliest.HasValue || dateTime.Value < this.Earliest.Value)
                this.Earliest = dateTime;
        }
        public void SetIfLater(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return;
            if (!this.Latest.HasValue || dateTime.Value > this.Latest.Value)
                this.Latest = dateTime;
        }
    }
    public class StatsSummaryRange : IStatsRange
    {
        public IQueryable<IDatedObject> Summaries { get; set; }

        public StatsSummaryRange(IQueryable<IDatedObject> summaries)
        {
            this.Summaries = summaries;
        }

        private bool? _anySums;
        private bool AnySums
        {
            get
            {
                if (!_anySums.HasValue)
                {
                    _anySums = (Summaries != null && Summaries.Any());
                }
                return _anySums.Value;
            }
        }

        private DateTime? _earliest;
        public DateTime? Earliest
        {
            get
            {
                if (!_earliest.HasValue && this.AnySums)
                {
                    _earliest = Summaries.Min(s => s.Date);
                }
                return _earliest;
            }
        }
        private DateTime? _latest;
        public DateTime? Latest
        {
            get
            {
                if (!_latest.HasValue && this.AnySums)
                {
                    _latest = Summaries.Max(s => s.Date);
                }
                return _latest;
            }
        }
    }

    public class ConvRange : IStatsRange
    {
        public IQueryable<Conv> Convs { get; set; }

        public ConvRange(IQueryable<Conv> convs)
        {
            this.Convs = convs;
        }

        private bool earliestSet;
        private DateTime? _earliest;
        public DateTime? Earliest
        {
            get
            {
                if (!earliestSet)
                {
                    _earliest = (Convs == null || !Convs.Any()) ? null : (DateTime?)Convs.Min(s => s.Time);
                    earliestSet = true;
                }
                return _earliest;
            }
        }

        private bool latestSet;
        private DateTime? _latest;
        public DateTime? Latest
        {
            get
            {
                if (!latestSet)
                {
                    _latest = (Convs == null || !Convs.Any()) ? null : (DateTime?)Convs.Max(s => s.Time);
                    latestSet = true;
                }
                return _latest;
            }
        }
    }
}
