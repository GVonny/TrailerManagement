using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class CreateSortImage
    {
        public IEnumerable<SortList> Trailers { get; set; }
        
        [Required]
        public HttpPostedFile ImageFile { get; set; }
    }
}