using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Entities.TD.DBM
{
    public class DailyLocationStat
    {
        public DateTime Date { get; set; }
        public int InsertionOrderID { get; set; }
        public virtual InsertionOrder InsertionOrder { get; set; }

        public int CityID { get; set; }
        public virtual City City { get; set; }
        public int RegionID { get; set; }
        public virtual Region Region { get; set; }
        public int DMACode { get; set; }
        public virtual DMA DMA { get; set; }

        [StringLength(8)]
        public string CountryAbbrev { get; set; }

        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
    }

    public class City
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CityID { get; set; }
        public string Name { get; set; }
    }

    public class Region
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RegionID { get; set; }
        public string Name { get; set; }
    }

    public class DMA
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DMACode { get; set; }
        public string Name { get; set; }
    }

}
