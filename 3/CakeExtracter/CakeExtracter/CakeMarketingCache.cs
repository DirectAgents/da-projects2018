using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CakeExtracter.Common;
using ClientPortal.Web.Models.Cake;

namespace CakeExtracter
{
    public class CakeMarketingCache : ICakeMarketingCache
    {
       private readonly XmlSerializer clicksSerializer = new XmlSerializer(typeof(click[]));

        public CakeMarketingCache()
        {
            Location = @"c:\CakeCache";
            ItemsPerFile = 10000;
        }

        public bool Enabled { get; set; }
        public string Location { get; set; }
        public int ItemsPerFile { get; set; }

        #region ICakeMarketingCache members
        public void PutClicks(List<click> clicks)
        {
            if (Enabled)
            {
                Console.WriteLine("Caching {0} clicks..", clicks.Count);

                foreach (var advertiserGroup in clicks.GroupBy(c => c.advertiser.advertiser_id))
                {
                    int advertiserId = advertiserGroup.Key;
                    foreach (var dateGroup in advertiserGroup.GroupBy(c => c.click_date.Date))
                    {
                        var date = dateGroup.Key;
                        string dirName = InitDirectory(date, advertiserId);
                        string fileName = FileName(date, advertiserId);

                        int i = 0;
                        foreach (var clicksSet in dateGroup.InBatches(ItemsPerFile))
                        {
                            var filePath = dirName + string.Format(fileName, ++i);

                            Console.WriteLine("Caching {0} clicks to {1}..", clicksSet.Count, filePath);

                            using (var writer = File.CreateText(filePath))
                            {
                                clicksSerializer.Serialize(writer, clicksSet.ToArray());
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region ICakeMarketing Members
        public List<click> Clicks(int advertiserId, DateTime date)
        {
            var dirInfo = new DirectoryInfo(DirName(date, advertiserId));
            if (dirInfo.Exists)
            {
                var files = dirInfo.EnumerateFiles();
                var clicks = files.SelectMany(c =>
                    {
                        Console.WriteLine("Reading clicks from cached file {0}", c.FullName);

                        using (var stream = c.OpenRead())
                        {
                            var deserializedClicks = (click[])clicksSerializer.Deserialize(stream);

                            Console.WriteLine("Deserialized {0} clicks..", deserializedClicks.Length);

                            return deserializedClicks;
                        }
                    });
                return clicks.ToList();
            }
            return null;
        }

        public List<conversion> Conversions(int advertiserId, DateTime startDate)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Helpers
        private string InitDirectory(DateTime date, int advertiserId)
        {
            var dirInfo = new DirectoryInfo(DirName(date, advertiserId));
            if (dirInfo.Exists)
                dirInfo.Delete(true);
            dirInfo.Create();
            return dirInfo.FullName;
        }

        private string DirName(DateTime date, int advertiserId)
        {
            string dirName = string.Format(@"{0}\{1}\{2}\", Location, advertiserId, DateString(date));
            return dirName;
        }

        private string FileName(DateTime date, int advertiserId)
        {
            var dateString = DateString(date);
            string fileName = "clicks_" + advertiserId + "_" + dateString + "_{0}.xml";
            return fileName;
        }

        private string DateString(DateTime date)
        {
            string dateString = string.Format("{0}-{1}-{2}", date.Year, date.Month, date.Day);
            return dateString;
        }
        #endregion
    }
}