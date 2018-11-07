using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class EditActiveTrailer
    {
        [Required]
        [StringLength(50)]
        [DisplayName("Trailer Number")]
        public string TrailerNumber { get; set; }
        [StringLength(50)]
        [DisplayName("Trailer Status")]
        public string TrailerStatus { get; set; }
        [StringLength(50)]
        [DisplayName("Load Status")]
        public string LoadStatus { get; set; }
        [StringLength(50)]
        public string Customer { get; set; }
        [StringLength(50)]
        [DisplayName("Order Date")]
        public string OrderDate { get; set; }
        [StringLength(50)]
        [DisplayName("Order Number")]
        public string OrderNumber { get; set; }
        [StringLength(50)]
        [DisplayName("Location Status")]
        public string LocationStatus { get; set; }
        [StringLength(50)]
        [DisplayName("Trailer Location")]
        public string TrailerLocation { get; set; }
        [StringLength(100)]
        public string Notes { get; set; }
        [StringLength(50)]
        public string Tags { get; set; }
        [StringLength(50)]
        [DisplayName("Date Modified")]
        public string DateModified { get; set; }
    }
}