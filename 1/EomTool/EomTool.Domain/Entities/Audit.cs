//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EomTool.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Audit
    {
        public long AuditID { get; set; }
        public System.DateTime AuditDate { get; set; }
        public string HostName { get; set; }
        public string SysUser { get; set; }
        public string Application { get; set; }
        public string TableName { get; set; }
        public string Operation { get; set; }
        public string SQLStatement { get; set; }
        public string PrimaryKey { get; set; }
        public string RowDescription { get; set; }
        public string SecondaryRow { get; set; }
        public string ColumnName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public Nullable<int> RowVersion { get; set; }
    }
}
