using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TrailerManagement.Models
{
    public class CreateInactiveTrailer
    {
        [Required]
        [StringLength(10)]
        [DisplayName("Trailer Number")]
        public string TrailerNumber { get; set; }
        [StringLength(15)]
        [DisplayName("DOT Date")]
        public string DOTDate { get; set; }
        [StringLength(30)]
        [DisplayName("Trailer Status")]
        public string Status { get; set; }
        [StringLength(25)]
        [DisplayName("Lease Info")]
        public string Leased { get; set; }
        [StringLength(5)]
        [DisplayName("Insurance Card")]
        public string InsuranceCard { get; set; }
        [StringLength(5)]
        public string Insured { get; set; }
        [StringLength(5)]
        public string Title { get; set; }
        [StringLength(255)]
        [DisplayName("Plate Number")]
        public string PlateNumber { get; set; }
        [StringLength(10)]
        [DisplayName("Vin Number")]
        public string VinNumber { get; set; }
        [StringLength(25)]
        public string Manufacturer { get; set; }
        [StringLength(25)]
        [DisplayName("Manufacture Year")]
        public string ManufactureYear { get; set; }
        [StringLength(4)]
        [DisplayName("Trailer Length")]
        public string TrailerLength { get; set; }
        [StringLength(3)]
        public string Issues { get; set; }
        [StringLength(15)]
        [DisplayName("Date Modified")]
        public string DateModified { get; set; }

        public InactiveTrailerList MapToTruck()
        {
            InactiveTrailerList trailer = new InactiveTrailerList()
            {
                TrailerNumber = this.TrailerNumber,
                DOTDate = this.DOTDate,
                TrailerStatus = this.Status,
                Leased = this.Leased,
                InsuranceCard = this.InsuranceCard,
                Insured = this.Insured,
                Title = this.Title,
                PlateNumber = this.PlateNumber,
                VinNumber = this.VinNumber,
                Manufacturer = this.Manufacturer,
                ManufactureYear = this.ManufactureYear,
                TrailerLength = this.TrailerLength,
                Issues = this.Issues,
                DateModified = this.DateModified
            };
            return trailer;
        }
    }
}