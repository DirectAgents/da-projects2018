
namespace Yahoo
{
    static class DateTypeId
    {
        public const int TODAY = 1;
        public const int YESTERDAY = 2;
        public const int THIS_WEEK = 3;
        public const int LAST_7_DAYS = 4;
        public const int LAST_WEEK = 5;
        public const int LAST_14_DAYS = 6;
        public const int MONTH_TO_DATE = 7;
        public const int LAST_30_DAYS = 8;
        public const int LAST_MONTH = 9;
        public const int ALL_TIME = 10;
        public const int CUSTOM_RANGE = 11;
    }

    static class IntervalTypeId
    {
        public const int CUMULATIVE = 1;
        public const int DAY = 2;
        public const int MONTH = 3;
        public const int WEEK = 4;
        public const int HOUR = 5;
    }

    static class Dimension
    {
        //...
        public const int ADVERTISER = 4;
        public const int CAMPAIGN = 5;
        public const int LINE = 6;
        public const int AD = 7;
        public const int CREATIVE = 8;
        public const int PIXEL_PARAMETER = 103;
        //...
    }

    static class Metric
    {
        public const int IMPRESSIONS = 1;
        public const int CLICKS = 2;
        public const int CONVERSIONS = 23;
        public const int ADVERTISER_SPENDING = 44;
        public const int VIEW_THROUGH_CONVERSIONS = 109;
        public const int CLICK_THROUGH_CONVERSIONS = 110;
        public const int ROAS_ACTION_VALUE = 137;
        //...
    }

    static class Timezone
    {
        public const string NEW_YORK = "America/New_York";
        public const string LOS_ANGELES = "America/Los_Angeles";
    }

    static class Currency
    {
        public const int SEAT = 1;
        public const int ADVERTISER = 2;
        public const int CAMPAIGN = 3;
        public const int USD = 4;
    }
}
