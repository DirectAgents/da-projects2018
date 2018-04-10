using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Models
{
    public class ReportingVM
    {
        public Campaign Campaign { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public IEnumerable<ColumnConfig> ColumnConfigs { get; set; }
    }

    public class ColumnConfig
    {
        public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string Format_Date = "dd/mm/yyyy";
        public const string Format_Integer = "#,0";
        public const string Format_DollarCents = "$#,0.00";
        public const string Format_Percent2Dec = "#,0.00%";

        public string PropName { get; set; }
        public string DisplayName { get; set; }
        public string Letter { get; set; }
        public string Format { get; set; }
        public string KendoType { get; set; }

        public ColumnConfig(string propName, string displayName, char letter, string format, string kendoType = "number")
        {
            this.PropName = propName;
            this.DisplayName = displayName;
            this.Letter = new String(letter, 1);
            this.Format = format;
            this.KendoType = kendoType;
        }

        public static IEnumerable<ColumnConfig> BasicColumns() {
            var cols = new List<ColumnConfig>();
            int letter = 0;
            cols.Add(new ColumnConfig("Date", null, Alphabet[letter++], ColumnConfig.Format_Date, kendoType: "date"));
            cols.Add(new ColumnConfig("Impressions", null, Alphabet[letter++], ColumnConfig.Format_Integer));
            cols.Add(new ColumnConfig("Clicks", null, Alphabet[letter++], ColumnConfig.Format_Integer));
            cols.Add(new ColumnConfig("TotalConv", null, Alphabet[letter++], ColumnConfig.Format_Integer));
            cols.Add(new ColumnConfig("PostClickConv", null, Alphabet[letter++], ColumnConfig.Format_Integer));
            cols.Add(new ColumnConfig("PostViewConv", null, Alphabet[letter++], ColumnConfig.Format_Integer));
            cols.Add(new ColumnConfig("Spend", null, Alphabet[letter++], ColumnConfig.Format_DollarCents));
            cols.Add(new ColumnConfig("CTR", null, Alphabet[letter++], ColumnConfig.Format_Percent2Dec));
            cols.Add(new ColumnConfig("CPA", null, Alphabet[letter++], ColumnConfig.Format_DollarCents));
            return cols;
        }
    }
}