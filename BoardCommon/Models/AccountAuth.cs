using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BoardCommon.Models
{
    public class AccountAuth
    {
        [Required]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "username length should be between 5 and 30 characters")]
        public string Login { get; set; }
        [Required]
        [StringLength(30, MinimumLength = 5)]
        public string Password { get; set; }

        public string GetHashedPassword()
        {
            var hasher = new PasswordHasher<AccountAuth>();
            return hasher.HashPassword(this, Password);
        }
    }
}