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
    
    public partial class PublisherNote
    {
        public int id { get; set; }
        public string publisher_name { get; set; }
        public string note { get; set; }
        public string added_by_system_user { get; set; }
        public System.DateTime created { get; set; }
    }
}