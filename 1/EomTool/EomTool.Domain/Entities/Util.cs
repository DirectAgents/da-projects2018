using System;
using System.Globalization;

namespace EomTool.Domain.Entities
{
    //enum UnitTypes { CPA = 4, ReserveFund = 6, CPM, CPC, RevShare, CoReg, Flat, Sale = 13, AffiliateProgramMgmt, PPC, CPI, OPM, TradingDesk, SEO, Creative, CreditCheckFee }
    //enum Currencies { USD = 1, GBP, EUR, CAD, AUD }

    public static class Util
    {
        public static bool ParseMoney(string money, out decimal amount, out string currency)
        {
            bool success = false;
            currency = null;
            if (Decimal.TryParse(money, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("en-US"), out amount))
            {
                currency = "USD";
                success = true;
            }
            else if (Decimal.TryParse(money, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("en-GB"), out amount))
            {
                currency = "GBP";
                success = true;
            }
            else if (Decimal.TryParse(money, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("de-DE"), out amount))
            {
                currency = "EUR";
                success = true;
            }
            else if (Decimal.TryParse(money, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("en-AU"), out amount))
            {
                currency = "AUD";
                success = true;
            }
            // CAD?

            return success;
        }
    }
}
