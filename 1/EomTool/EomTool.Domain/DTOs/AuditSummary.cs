using System;

namespace EomTool.Domain.DTOs
{
    public class AuditSummary
    {
        public DateTime? Date { get; set; }
        public int NumUpdates { get; set; }
        public int NumInserts { get; set; }
        public int NumDeletes { get; set; }
    }
}
