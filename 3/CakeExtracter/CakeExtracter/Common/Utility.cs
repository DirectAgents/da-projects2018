using System;

namespace CakeExtracter.Common
{
    public class Utility
    {
        // unixTimeStamp is in microseconds (1/1,000,000 second)
        // a tick is 1/10,000,000 second
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp, int convertToTimezoneOffset = 0)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddTicks(unixTimeStamp * 10);
            if (convertToTimezoneOffset == 0)
                return dtDateTime;
            else
                return dtDateTime.AddHours(convertToTimezoneOffset);
        }
    }
}
