using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class EditUser
    {
        [Required]
        [StringLength(20)]
        public string Username { get; set; }
        [StringLength(20)]
        public string Password { get; set; }
        [Required]
        public string Department { get; set; }
        [Required]
        [StringLength(20)]
        public string Permission { get; set; }
        [Required]
        [StringLength(15)]
        [DisplayName("Name")]
        public string FirstName { get; set; }
    }
}