using System;

namespace AdRoll.Entities
{
    public class Ad
    {
        public string eid { get; set; }
        public string name { get; set; }
        //public string ad_format { get; set; }
        //public string ad_format_name { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string src { get; set; }
        public string html { get; set; } //for adform creatives, etc
        public string body { get; set; }
        public string headline { get; set; }
        public string message { get; set; }
        public string destination_url { get; set; }

        //public string type { get; set; }
        public DateTime created_date { get; set; }
        public DateTime updated_date { get; set; }
        public string status { get; set; }
        // is_active, etc...
    }
}
