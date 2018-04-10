using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdRoll.Entities
{
    public class AttributionSummary
    {
        public DateTime date { get; set; }
        public int click_throughs { get; set; }
        public int view_throughs { get; set; }
        public double click_revenue { get; set; }
        public double view_revenue { get; set; }
    }
}
