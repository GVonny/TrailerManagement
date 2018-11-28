using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class CreatePalletType
    {
        [Required]
        [DisplayName("Part Number")]
        public string PartNumber { get; set; }
        [Required]
        public string Description { get; set; }
        public string Type { get; set; }
        [DisplayName("Tags Required")]
        public string TagsRequired { get; set; }
        [DisplayName("Put Away Location")]
        public string PutAwayLocation { get; set; }

        public PalletType MapToType()
        {
            PalletType newPalletType = new PalletType()
            {
                PartNumber = this.PartNumber,
                Description = this.PartNumber,
                Type = this.Type,
                TagsRequired = this.TagsRequired,
                PutAwayLocation = this.PutAwayLocation,
                Status = "ACTIVE",
            };
            return newPalletType;
        }
    }
}