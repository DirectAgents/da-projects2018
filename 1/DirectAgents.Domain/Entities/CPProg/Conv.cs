using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.CPProg
{
    // TD Conversion
    public class Conv
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual ExtAccount ExtAccount { get; set; }

        public int? StrategyId { get; set; } // optional
        public virtual Strategy Strategy { get; set; }
        public int? TDadId { get; set; } // optional
        public virtual TDad TDad { get; set; }

        public DateTime Time { get; set; }
        [MaxLength(25)] // doesn't do client-validation; if needed, use StringLength instead
        public string ConvType { get; set; }
        public decimal ConvVal { get; set; }
        public int? CityId { get; set; } // optional
        [ForeignKey("CityId")]
        public virtual ConvCity City { get; set; }
        public string IP { get; set; }
        public string ExtData { get; set; }
    }

    public class ConvCity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual ConvCountry Country { get; set; }
    }
    public class ConvCountry
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
