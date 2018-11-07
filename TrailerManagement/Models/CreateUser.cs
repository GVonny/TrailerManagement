using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TrailerManagement.Models
{
    public class CreateUser
    {
        [Required]
        [StringLength(40)]
        public string Username { get; set; }
        [Required]
        [StringLength(20)]
        public string Password { get; set; }
        [Required]
        [StringLength(10)]
        public string Permission { get; set; }
        [Required]
        public string Department { get; set; }
        [Required]
        [StringLength(15)]
        [DisplayName("Name")]
        public string FirstName { get; set; }

        public User MapToUser()
        {
            User user = new User()
            {
                Username = this.Username,
                Password = this.Password,
                Permission = this.Permission,
                Department = this.Department,
                FirstName = this.FirstName
            };
            return user;
        }
    }
}