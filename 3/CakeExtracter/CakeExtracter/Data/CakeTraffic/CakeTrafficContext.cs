using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace CakeExtracter.Data.CakeTraffic
{
    class CakeTrafficContext : DbContext
    {
        public DbSet<CakeTraffic> Traffics { get; set; }
        public DbSet<CakeTrafficAdvertiser> Advertisers { get; set; }
        public DbSet<CakeTrafficOffer> Offers { get; set; }
        public DbSet<CakeTrafficAffiliate> Affiliates { get; set; }
        public DbSet<CakeTrafficRate> Rates { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }

    class CakeTraffic
    {
        public CakeTraffic()
        {
            AdvertiserRates = new HashSet<CakeTrafficRate>();
            AffiliateRates = new HashSet<CakeTrafficRate>();
        }

        [Key]
        public int TrafficId { get; set; }
        public CakeTrafficAdvertiser Advertiser { get; set; }
        public CakeTrafficOffer Offer { get; set; }
        public CakeTrafficAffiliate Affiliate { get; set; }
        public decimal Conversions { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public virtual ICollection<CakeTrafficRate> AdvertiserRates { get; set; }
        public virtual ICollection<CakeTrafficRate> AffiliateRates { get; set; }
    }

    class CakeTrafficRate
    {
        [Key]
        public int RateId { get; set; }
        public decimal Price { get; set; }
        public decimal Conversions { get; set; }
        public int TrafficId { get; set; }
        public virtual CakeTraffic Traffic { get; set; }
    }

    class CakeTrafficAdvertiser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int AdvertiserId { get; set; }
        public string AdvertiserName { get; set; }
    }

    class CakeTrafficOffer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int OfferId { get; set; }
        public string OfferName { get; set; }
    }

    class CakeTrafficAffiliate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int AffiliateId { get; set; }
        public string AffiliateName { get; set; }
    }
}
