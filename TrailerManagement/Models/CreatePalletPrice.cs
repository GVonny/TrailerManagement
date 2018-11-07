using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class CreatePalletPrice
    {
        public int PalletPriceGUID { get; set; }
        public string VendorNumber { get; set; }
        [Required]
        [DisplayName("Vendor Name")]
        public string VendorName { get; set; }
        [Required]
        [DisplayName("Part Number")]
        public string PartNumber { get; set; }
        public string Description { get; set; }
        [Required]
        [DisplayName("Purchase Price")]
        public string PurchasePrice { get; set; }

        public PalletPrice MapToPrice()
        {
            PalletPrice newPreset = new PalletPrice()
            {
                VendorNumber = this.VendorNumber,
                VendorName = this.VendorName,
                PartNumber = this.PartNumber,
                Description = this.Description,
                PurchasePrice = this.PurchasePrice,
            };
            return newPreset;
        }
    }
}