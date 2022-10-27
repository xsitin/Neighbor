using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Common.Models;

public class AccountAuth
{
    [Required]
    [StringLength(30, MinimumLength = 5, ErrorMessage = "Длина логина должна быть не меньше 5 и не больше 30 символов")]
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
