using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Sample.Core.Identity.Data.Enities
{
    public class ApplicationUser :IdentityUser
    {
        [Required]
        [MaxLength(200)]
        public String FirstName { get; set; }

        [Required]
        [MaxLength(250)]
        public String LastName { get; set; }

    }
}
