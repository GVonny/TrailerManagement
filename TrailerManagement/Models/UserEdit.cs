using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class UserEdit
    {
        [Required]
        [StringLength(20)]
        public string Username { get; set; }
        [Required]
        [StringLength(20)]
        public string Password { get; set; }
        [Required]
        [StringLength(20)]
        public string Department { get; set; }
        [Required]
        [StringLength(20)]
        public string Permission { get; set; }
    }
}