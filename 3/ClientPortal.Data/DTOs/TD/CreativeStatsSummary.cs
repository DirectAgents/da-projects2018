using System;

namespace ClientPortal.Data.DTOs.TD
{
    public class CreativeStatsSummary : StatsSummaryBase
    {
        public int CreativeID { get; set; }
        public string CreativeName { get; set; }

        public int Conv // used for sorting
        {
            get { return (Conversions > 0) ? 1 : 0; }
        }
    }
}
