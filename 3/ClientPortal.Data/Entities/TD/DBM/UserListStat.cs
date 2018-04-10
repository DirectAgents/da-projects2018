using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Entities.TD.DBM
{
    public class UserListRun
    {
        public int ID { get; set; }

        public int? InsertionOrderID { get; set; }
        public virtual InsertionOrder InsertionOrder { get; set; }

        public DateTime Date { get; set; } //TODO: add StartDate or something that allows us to record 'Date range'
        public string Name { get; set; }
        public string Bucket { get; set; }
        public virtual ICollection<UserListStat> Lists { get; set; }
    }

    public class UserListStat
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserListID { get; set; }

        public int UserListRunID { get; set; }
        public virtual UserListRun UserListRun { get; set; }

        public string UserListName { get; set; }
        public float Cost { get; set; }
        public long EligibleCookies { get; set; }
        public float MatchRatio { get; set; }
        public long PotentialImpressions { get; set; }
        public int Uniques { get; set; } // how to represent '<1000' ... with '999'?
    }
}
