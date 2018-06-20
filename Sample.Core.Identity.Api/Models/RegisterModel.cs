using System;
using System.ComponentModel.DataAnnotations;

namespace Sample.Core.Identity.Api.Models
{
    public class RegisterModel:LoginModel
    {
        [Required]
        [StringLength(200)]
        public String FirstName { get; set; }

        [Required]
        [StringLength(250)]
        public String LastName { get; set; }

        [Required]
        [EmailAddress]
        public String Email { get; set; }

        [Required]
        [Compare("Password")]
        public String PasswordConfirmation { get; set; }
    }
}
