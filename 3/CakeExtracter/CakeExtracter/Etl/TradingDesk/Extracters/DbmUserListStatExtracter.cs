using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using ClientPortal.Data.Entities.TD.DBM;
using CsvHelper;
using CsvHelper.Configuration;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class DbmUserListStatExtracter : Extracter<UserListStatRow>
    {
        private readonly UserListRun userListRun;

        public DbmUserListStatExtracter(UserListRun userListRun)
        {
            this.userListRun = userListRun;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting UserListStats for UserListRun {0}", userListRun.ID);
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<UserListStatRow> EnumerateRows()
        {
            var credential = DbmCloudStorageExtracter.CreateCredential();
            var service = DbmCloudStorageExtracter.CreateStorageService(credential);

            var request = service.Objects.List(userListRun.Bucket);
            var bucketObjects = request.Execute();

            string dateString = userListRun.Date.ToString("yyyy-MM-dd");
            var reportObject = bucketObjects.Items.Where(i => i.Name.Contains(dateString)).FirstOrDefault();

            if (reportObject != null)
            {
                var stream = DbmCloudStorageExtracter.GetStreamForCloudStorageObject(reportObject, credential);
                using (var reader = new StreamReader(stream))
                {
                    foreach (var row in EnumerateRowsStatic(reader))
                        yield return row;
                }
            }
        }

        public static IEnumerable<UserListStatRow> EnumerateRowsStatic(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                csv.Configuration.SkipEmptyRecords = true;
                csv.Configuration.RegisterClassMap<UserListStatRowMap>();

                while (csv.Read())
                {
                    UserListStatRow row;
                    try
                    {
                        row = csv.GetRecord<UserListStatRow>();
                    }
                    catch (CsvHelperException)
                    {
                        continue; // if error converting the row
                    }
                    //if (row.Uniques != "< 1000")
                    if (row.Uniques != "< 1000" && row.MatchRatio > 100)
                        yield return row;
                }
            }
        }
    }

    public sealed class UserListStatRowMap : CsvClassMap<UserListStatRow>
    {
        public UserListStatRowMap()
        {
            Map(m => m.UserListID).Name("User List ID").NameIndex(1); // second one
            Map(m => m.UserListName).Name("User List").NameIndex(1); // second one
            Map(m => m.Cost).Name("User List Cost (USD)").NameIndex(1); // second one
            Map(m => m.EligibleCookies).Name("Eligible Cookies on Third Party List");
            Map(m => m.MatchRatio).Name("Match Ratio");
            Map(m => m.PotentialImpressions).Name("Potential Impressions");
            Map(m => m.Uniques).Name("Uniques");
        }
    }
    public class UserListStatRow
    {
        public int UserListID { get; set; }
        public string UserListName { get; set; }
        //public float Cost { get; set; }
        public string Cost { get; set; } // could be "Unknown"
        public long EligibleCookies { get; set; }
        public float MatchRatio { get; set; }
        public long PotentialImpressions { get; set; }
        public string Uniques { get; set; } // could be "<1000"
    }
}
