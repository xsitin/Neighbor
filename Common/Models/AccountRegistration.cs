using System.ComponentModel.DataAnnotations;

namespace Common.Models;

public class AccountRegistration:AccountAuth
{
    [Required]
    public string Name { get; set; }
}