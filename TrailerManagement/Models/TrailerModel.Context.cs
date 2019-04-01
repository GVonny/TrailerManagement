﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TrailerManagement.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TrailerEntities : DbContext
    {
        public TrailerEntities()
            : base("name=TrailerEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ActiveInventoryLocation> ActiveInventoryLocations { get; set; }
        public virtual DbSet<ActiveLocationRow> ActiveLocationRows { get; set; }
        public virtual DbSet<ActiveTrailerList> ActiveTrailerLists { get; set; }
        public virtual DbSet<CodeViolation> CodeViolations { get; set; }
        public virtual DbSet<CompletedSort> CompletedSorts { get; set; }
        public virtual DbSet<CustomersAndVendor> CustomersAndVendors { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<InactiveTrailerList> InactiveTrailerLists { get; set; }
        public virtual DbSet<InventoryRowStack> InventoryRowStacks { get; set; }
        public virtual DbSet<MasterStack> MasterStacks { get; set; }
        public virtual DbSet<PalletPrice> PalletPrices { get; set; }
        public virtual DbSet<PalletType> PalletTypes { get; set; }
        public virtual DbSet<Payout> Payouts { get; set; }
        public virtual DbSet<SafetyCode> SafetyCodes { get; set; }
        public virtual DbSet<SafetyConcern> SafetyConcerns { get; set; }
        public virtual DbSet<SortImage> SortImages { get; set; }
        public virtual DbSet<SortList> SortLists { get; set; }
        public virtual DbSet<SortListTest> SortListTests { get; set; }
        public virtual DbSet<TrailerList> TrailerLists { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<StackNote> StackNotes { get; set; }
        public virtual DbSet<MasterStacksTest> MasterStacksTests { get; set; }
        public virtual DbSet<DriverConcernImage> DriverConcernImages { get; set; }
        public virtual DbSet<DriverConcern> DriverConcerns { get; set; }
        public virtual DbSet<DriverConcernsList> DriverConcernsLists { get; set; }
        public virtual DbSet<ProductionEmployee> ProductionEmployees { get; set; }
        public virtual DbSet<ProductionStack> ProductionStacks { get; set; }
        public virtual DbSet<Workstation> Workstations { get; set; }
        public virtual DbSet<ProductionWorkOrder> ProductionWorkOrders { get; set; }
        public virtual DbSet<ProductionHour> ProductionHours { get; set; }
    }
}
