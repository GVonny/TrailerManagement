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
    
    public partial class payouttemp
    {
        public long payoutguid { get; set; }
        public Nullable<long> sortguid { get; set; }
        public string trailernumber { get; set; }
        public string vendor { get; set; }
        public Nullable<int> vendornumber { get; set; }
        public string ordernumber { get; set; }
        public string purchaseordernumber { get; set; }
        public string invoicenumber { get; set; }
        public string packinglistnumber { get; set; }
        public string billoflading { get; set; }
        public string status { get; set; }
        public string datearrived { get; set; }
        public Nullable<double> timetosort { get; set; }
        public string datecompleted { get; set; }
    }
}
