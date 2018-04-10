using ClientPortal.Data.Contexts;
using ClientPortal.Data.DTOs;
using System.Globalization;
namespace CakeExtracter.Reports
{
    public partial class SearchReportRuntimeTextTemplate : SearchReportRuntimeTextTemplateBase
    {
        public SearchProfile SearchProfile { get; set; }
        
        public SearchStat Line1stat { get; set; }
        public SearchStat Line2stat { get; set; }
        public SimpleSearchStat ChangeStat { get; set; }

        public string AcctMgrName { get; set; }
        public string AcctMgrEmail { get; set; }

        private NumberFormatInfo _noParensFormatInfo;
        public NumberFormatInfo NoParensFormatInfo
        {
            get
            {
                if (_noParensFormatInfo == null)
                {
                    _noParensFormatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                    _noParensFormatInfo.CurrencyNegativePattern = 1;
                }
                return _noParensFormatInfo;
            }
        }

        private string NoChangeSymbol = "~";

        public string Currency(decimal val, bool plusMinus = false)
        {
            if (!plusMinus)
                return val.ToString("C");

            if (val > 0)
                return "+" + val.ToString("C");
            else if (val < 0)
                return val.ToString("C", NoParensFormatInfo);
            else
                return NoChangeSymbol;
        }

        public string Integer(int val, bool plusMinus = false)
        {
            if (!plusMinus)
                return val.ToString("N0");

            if (val > 0)
                return "+" + val.ToString("N0");
            else if (val < 0)
                return val.ToString("N0");
            else
                return NoChangeSymbol;
        }

        public string Decimal(decimal val, bool plusMinus = false, int places = 2)
        {
            if (!plusMinus)
                return val.ToString("N" + places);

            if (val > 0)
                return "+" + val.ToString("N" + places);
            else if (val < 0)
                return val.ToString("N" + places);
            else
                return NoChangeSymbol;
        }
    }
}
