using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class CreateActiveTrailer
    {
        [Required]
        [StringLength(10)]
        [DisplayName("Trailer Number")]
        public string TrailerNumber { get; set; }
        [StringLength(15)]
        [DisplayName("Trailer Status")]
        public string TrailerStatus { get; set; }
        [StringLength(30)]
        [DisplayName("Load Status")]
        public string LoadStatus { get; set; }
        [StringLength(25)]
        public string Customer { get; set; }
        [StringLength(15)]
        [DisplayName("Order Date")]
        public string OrderDate { get; set; }
        [StringLength(15)]
        [DisplayName("Order Number")]
        public string OrderNumber { get; set; }
        [StringLength(15)]
        [DisplayName("Location Status")]
        public string LocationStatus { get; set; }
        [StringLength(255)]
        [DisplayName("Trailer Location")]
        public string TrailerLocation { get; set; }
        [StringLength(50)]
        public string Notes { get; set; }
        [StringLength(25)]
        public string Tags { get; set; }
        [StringLength(25)]
        [DisplayName("Date Modified")]
        public string DateModified { get; set; }

        public ActiveTrailerList MapToTruck()
        {
            ActiveTrailerList trailer = new ActiveTrailerList()
            {
                TrailerNumber = this.TrailerNumber,
                TrailerStatus = this.TrailerStatus,
                LoadStatus = this.LoadStatus,
                Customer = this.Customer,
                OrderDate = this.OrderDate,
                OrderNumber = this.OrderNumber,
                LocationStatus = this.LocationStatus,
                TrailerLocation = this.TrailerLocation,
                Notes = this.Notes,
                Tags = this.Tags,
                DateModified = this.DateModified
            };
            return trailer;
        }
    }
}