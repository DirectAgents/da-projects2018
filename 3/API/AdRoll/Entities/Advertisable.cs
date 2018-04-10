using System;

namespace AdRoll.Entities
{
    public class Advertisable
    {
        public string eid { get; set; }
        public string name { get; set; }
        public string currency { get; set; }
        public bool is_active { get; set; }
        public string status { get; set; }
        public DateTime created_date { get; set; }
        public DateTime updated_date { get; set; }
    }
}
