using System;

namespace DBM.Entities
{
    public class Creative
    {
        // https://developers.google.com/bid-manager/guides/entity-read/format-v2#creative
        public EntityCommonData common_data { get; set; }
        public int advertiser_id { get; set; }
        public int width_pixels { get; set; }
        public int height_pixels { get; set; }
        public CreativeType creative_type { get; set; }
    }

    public enum CreativeType
    {
        IMAGE = 0,
        EXPANDABLE = 1,
        VIDEO = 2,
        MOBILE = 3,
        Facebook_Exchange_Right_Hand_Side_Ads = 4,
        Facebook_Exchange_Page_Post_News_Feed_Ads = 5
    }
}
