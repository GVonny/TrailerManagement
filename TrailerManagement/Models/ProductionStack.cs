//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class ProductionStack
    {
        public long ProductionStackGUID { get; set; }
        public Nullable<int> WorkstationNumber { get; set; }
        public string EmployeeName { get; set; }
        public Nullable<int> EmployeeBadgeNumber { get; set; }
        public string PartNumber { get; set; }
        public Nullable<int> StackQuantity { get; set; }
        public string TimeStamp { get; set; }
        public Nullable<int> WorkOrderNumber { get; set; }
        public Nullable<bool> IsEndOfDay { get; set; }
        public string Date { get; set; }
    }
}
