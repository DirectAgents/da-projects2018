using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBM.Entities
{
    public class EntityCommonData
    {
        // https://developers.google.com/bid-manager/guides/entity-read/format-v2#entitycommondata
        public int id { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public string integration_code { get; set; }
    }
}
