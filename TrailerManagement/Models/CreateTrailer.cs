using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class CreateTrailer
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

        [StringLength(10)]
        [DisplayName("Title")]
        public string Title { get; set; }

        [StringLength(255)]
        [DisplayName("Plate Number")]
        public string PlateNumber { get; set; }

        [StringLength(25)]
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

        [StringLength(100)]
        public string Issues { get; set; }

        [StringLength(15)]
        [DisplayName("Date Modified")]
        public string DateModified { get; set; }

        [StringLength(50)]
        [DisplayName("Location")]
        public string TrailerLocation { get; set; }

        public TrailerList MapToTruck()
        {
            TrailerList trailer = new TrailerList()
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
                TrailerLocation = this.TrailerLocation,
                TrailerLength = this.TrailerLength,
                Issues = this.Issues,
                DateModified = this.DateModified
            };
            return trailer;
        }
    }
}