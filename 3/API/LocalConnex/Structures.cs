using System;

namespace LocalConnex
{
    public struct CallSearchParams
    {
        public DateTime start;
        public DateTime end;
    }

    public struct Call
    {
        public string keyword;
        public string call_status;
        public int numlookup_p; //new
        public string forwardno;
        public string rating;
        public string acct;
        public string cmpid;
        public string grpid;
        public object answer_offset; //int?
        public string assigned_to;
        public string inboundno;
        public bool recorded;
        public string inbound_ext;
        public DateTime call_start;
        public string disposition;
        public string caller_number;
        public object ring_duration; //int?
        public string call_id;
        public string revenue;
        public int call_duration; //new
        public DateTime call_end;
        public string caller_name;
    }
}
