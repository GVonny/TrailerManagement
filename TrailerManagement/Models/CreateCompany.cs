using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class CreateCompany
    {
        public int CompanyGUID { get; set; }
        [DisplayName("Customer Number:")]
        public string CustomerNumber { get; set; }
        public string VendorNumber { get; set; }
        [Required]
        public string Name { get; set; }
        public string Type { get; set; }
        public string SortType { get; set; }
        public string EmailAddresses { get; set; }
        public string SortTypeDescription { get; set; }
        public string PhoneNumber { get; set; }
        public string ContactName { get; set; }
        public string PayoutDescription { get; set; }

        public CustomersAndVendor MapToCompany()
        {
            CustomersAndVendor company = new CustomersAndVendor()
            {
                CustomerNumber = this.CustomerNumber,
                VendorNumber = this.VendorNumber,
                Name = this.Name.ToUpper(),
                SortType = this.SortType,
                EmailAddresses = this.EmailAddresses,
                SortTypeDescription = this.SortTypeDescription,
                PhoneNumber = this.PhoneNumber,
                ContactName = this.ContactName,
                PayoutDescription = this.PayoutDescription,
            };

            if (this.VendorNumber == null && this.CustomerNumber != null)
            {
                company.Type = "Customer Only";
            }
            else if (this.CustomerNumber == null && this.VendorNumber != null)
            {
                company.Type = "Vendor Only";
            }
            else if(this.CustomerNumber != null && this.VendorNumber != null) 
            {
                company.Type = "Customer/Vendor";
            }
            return company;
        }
    }
}