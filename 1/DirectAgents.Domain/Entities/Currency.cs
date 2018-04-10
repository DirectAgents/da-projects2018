using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities
{
    public class CurrencyId
    {
        public const int NONE = 0;
        public const int USD = 1;
        public const int GBP = 2;
        public const int EUR = 3;
        public const int CAD = 4;
        public const int AUD = 5;
    }
    public class CurrencyAbbr
    {
        public const string NONE = "???"; // for display only
        public const string USD = "USD";
        public const string GBP = "GBP";
        public const string EUR = "EUR";
        public const string CAD = "CAD";
        public const string AUD = "AUD";
    }

    public class Currency
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Abbr { get; set; }
    }
}
