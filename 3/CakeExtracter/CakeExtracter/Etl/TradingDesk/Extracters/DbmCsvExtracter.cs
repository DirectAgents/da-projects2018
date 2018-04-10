using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    // DoubleClick BidManager Extracter
    public class DbmCsvExtracter : Extracter<DbmRowBase>
    {
        private readonly string csvFilePath;
        private readonly StreamReader streamReader;
        // if streamReader is not null, use it. otherwise use csvFilePath.

        private readonly bool wantCreativeDailySummaries;

        public DbmCsvExtracter(string csvFilePath, bool wantCreativeDailySummaries)
        {
            this.csvFilePath = csvFilePath;
            this.wantCreativeDailySummaries = wantCreativeDailySummaries;
        }

        public DbmCsvExtracter(StreamReader streamReader, bool wantCreativeDailySummaries)
        {
            this.streamReader = streamReader;
            this.wantCreativeDailySummaries = wantCreativeDailySummaries;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting DailySummaries from {0}", csvFilePath ?? "StreamReader");
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<DbmRowBase> EnumerateRows()
        {
            if (streamReader != null)
            {
                foreach (var row in EnumerateRowsStatic(streamReader, byCreative: wantCreativeDailySummaries))
                    yield return row;
            }
            else
            {
                using (StreamReader reader = File.OpenText(csvFilePath))
                {
                    foreach (var row in EnumerateRowsStatic(reader, byCreative: wantCreativeDailySummaries))
                        yield return row;
                }
            }
        }

        public static IEnumerable<DbmRowBase> EnumerateRowsStatic(StreamReader reader, bool byLineItem = false, bool byCreative = false, bool bySite = false)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                csv.Configuration.IgnoreHeaderWhiteSpace = true;
                csv.Configuration.SkipEmptyRecords = true;
                csv.Configuration.IgnoreReadingExceptions = true;

                if (byLineItem)
                {
                    csv.Configuration.RegisterClassMap<DbmRowWithLineItemMap>();

                    var csvRows = csv.GetRecords<DbmRowWithLineItem>().ToList();
                    for (int i = 0; i < csvRows.Count; i++)
                    {
                        yield return csvRows[i];
                    }
                }
                else if (byCreative)
                {
                    csv.Configuration.RegisterClassMap<DbmRowWithCreativeMap>();

                    //TESTING
                    //int testIO = 1946273; // bevel
                    //int testIO = 1844659; // coursehero
                    //int testIO = 1632789; // crackle
                    //int testIO = 2131348; // ellura
                    //int testIO = 2301762; // gaiam

                    var csvRows = csv.GetRecords<DbmRowWithCreative>().ToList();
                    for (int i = 0; i < csvRows.Count; i++)
                    {
                        //TESTING!!!
                        //if (csvRows[i].InsertionOrderID == testIO)
                        //    yield return csvRows[i];

                        yield return csvRows[i];
                    }
                }
                else if (bySite)
                {
                    csv.Configuration.RegisterClassMap<DbmRowWithSiteMap>();

                    var csvRows = csv.GetRecords<DbmRowWithSite>().ToList();
                    for (int i = 0; i < csvRows.Count; i++)
                    {
                        yield return csvRows[i];
                    }
                }
                else
                {
                    csv.Configuration.RegisterClassMap<DbmRowMap>();

                    var csvRows = csv.GetRecords<DbmRow>().ToList();
                    for (int i = 0; i < csvRows.Count; i++)
                    {
                        yield return csvRows[i];
                    }
                }
            }
        }

        private IEnumerable<DbmRowBase> GroupByDateAndEnumerate(List<DbmRow> csvRows)
        {
            var groupedRows = csvRows.GroupBy(r => new { r.Date, r.InsertionOrderID, r.InsertionOrder });
            foreach (var dayGroup in groupedRows)
            {
                var row = new DbmRow
                {
                    Date = dayGroup.Key.Date,
                    //Impressions = dayGroup.Sum(g => g.Impressions),
                    //Clicks = dayGroup.Sum(g => g.Clicks),
                    //TotalConversions =
                    //PostClickConversions = dayGroup.Sum(g => g.PostClickConversions),
                    //PostViewConversions = dayGroup.Sum(g => g.PostViewConversions),
                    //Revenue = dayGroup.Sum(g => g.Revenue)
                };
                yield return row;
            }
        }
    }

    public sealed class DbmRowMap : CsvClassMap<DbmRow>
    {
        public DbmRowMap()
        {
            Map(m => m.Date);
            Map(m => m.InsertionOrder);
            Map(m => m.InsertionOrderID);
            Map(m => m.Impressions);
            Map(m => m.Clicks);
            Map(m => m.TotalConversions);
            Map(m => m.PostClickConversions).Name("Post-ClickConversions");
            Map(m => m.PostViewConversions).Name("Post-ViewConversions");
            Map(m => m.PostClickRevenue).Name("DCMPost-ClickRevenue");
            Map(m => m.PostViewRevenue).Name("DCMPost-ViewRevenue");
            Map(m => m.Revenue).Name("Revenue(USD)"); // DA's revenue
        }
    }
    public sealed class DbmRowWithLineItemMap : CsvClassMap<DbmRowWithLineItem>
    {
        public DbmRowWithLineItemMap()
        {
            Map(m => m.Date);
            Map(m => m.InsertionOrder);
            Map(m => m.InsertionOrderID);
            Map(m => m.Impressions);
            Map(m => m.Clicks);
            Map(m => m.TotalConversions);
            Map(m => m.PostClickConversions).Name("Post-ClickConversions");
            Map(m => m.PostViewConversions).Name("Post-ViewConversions");
            Map(m => m.PostClickRevenue).Name("DCMPost-ClickRevenue");
            Map(m => m.PostViewRevenue).Name("DCMPost-ViewRevenue");
            Map(m => m.Revenue).Name("Revenue(USD)"); // DA's revenue
            Map(m => m.LineItem);
            Map(m => m.LineItemID);
        }
    }
    public sealed class DbmRowWithCreativeMap : CsvClassMap<DbmRowWithCreative>
    {
        public DbmRowWithCreativeMap()
        {
            Map(m => m.Date);
            Map(m => m.InsertionOrder);
            Map(m => m.InsertionOrderID);
            Map(m => m.Impressions);
            Map(m => m.Clicks);
            Map(m => m.TotalConversions);
            Map(m => m.PostClickConversions).Name("Post-ClickConversions");
            Map(m => m.PostViewConversions).Name("Post-ViewConversions");
            Map(m => m.Revenue).Name("Revenue(USD)"); // DA's revenue
            Map(m => m.Creative);
            Map(m => m.CreativeID);
            Map(m => m.CreativeSource);
            Map(m => m.CreativeWidth);
            Map(m => m.CreativeHeight);
            // CreativeStatus, etc...
        }
    }
    public sealed class DbmRowWithSiteMap : CsvClassMap<DbmRowWithSite>
    {
        public DbmRowWithSiteMap()
        {
            Map(m => m.Date).Name("Month");
            Map(m => m.InsertionOrder);
            Map(m => m.InsertionOrderID);
            Map(m => m.Impressions);
            Map(m => m.Clicks);
            Map(m => m.TotalConversions);
            Map(m => m.PostClickConversions).Name("Post-ClickConversions");
            Map(m => m.PostViewConversions).Name("Post-ViewConversions");
            Map(m => m.Revenue).Name("Revenue(USD)"); // DA's revenue
            Map(m => m.Site).Name("App/URL");
            Map(m => m.SiteID).Name("App/URLID");
        }
    }

    public class DbmRowBase
    {
        public DateTime Date { get; set; }

        public string InsertionOrder { get; set; }
        public int InsertionOrderID { get; set; }

        public string Impressions { get; set; } // int
        public string Clicks { get; set; } // int
        public string TotalConversions { get; set; } // int
        public string PostClickConversions { get; set; } // int
        public string PostViewConversions { get; set; } // int

        // DA's revenue
        public string Revenue { get; set; } // decimal
    }

    public class DbmRow : DbmRowBase
    {
        // the client's revenue
        public string PostClickRevenue { get; set; } // decimal
        public string PostViewRevenue { get; set; } // decimal
    }
    public class DbmRowWithLineItem : DbmRowBase
    {
        public string LineItem { get; set; }
        public string LineItemID { get; set; } //int

        // the client's revenue
        public string PostClickRevenue { get; set; } // decimal
        public string PostViewRevenue { get; set; } // decimal
    }
    public class DbmRowWithCreative : DbmRowBase
    {
        public string Creative { get; set; }
        public string CreativeID { get; set; } // int
        //public string DCMPlacementID { get; set; } // int
        //public string CreativeStatus { get; set; }
        public string CreativeSource { get; set; } // e.g. DCM, User
        //public string CreativeIntegrationCode { get; set; }
        public int CreativeWidth { get; set; }
        public int CreativeHeight { get; set; }
        //public string CreativeType { get; set; }
        //public string CreativeAttributes { get; set; } // e.g. 'Active View', 'None'
    }
    public class DbmRowWithSite : DbmRowBase
    {
        private string _site;
        public string Site
        {
            get { return _site; }
            set { _site = value.ToLower(); }
        }
        public string SiteID { get; set; } // int
    }
}
