using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class UserSignIn
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}