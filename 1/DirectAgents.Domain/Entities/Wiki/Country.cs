//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DirectAgents.Domain.Entities.Wiki
{
    using System;
    using System.Collections.Generic;
    
    public partial class Country
    {
        public Country()
        {
            this.Campaigns = new HashSet<Campaign>();
        }
    
        public string CountryCode { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<Campaign> Campaigns { get; set; }
    }
}