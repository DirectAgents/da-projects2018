﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class WikiContext : DbContext
    {
        public WikiContext()
            : base("name=WikiContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Conversion> Conversions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<DailySummary> DailySummaries { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<TrafficType> TrafficTypes { get; set; }
        public DbSet<Vertical> Verticals { get; set; }
    }
}