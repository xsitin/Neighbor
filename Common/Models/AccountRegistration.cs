namespace Common.Models;

using System.ComponentModel.DataAnnotations;

public class AccountRegistration : AccountAuth
{
    [Required] public string Name { get; set; }

    [Required]
    [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Некорректный адресс электронной почты")]
    public string Email { get; set; }

    [Required]
    [RegularExpression(@"^\+7\d{10}$", ErrorMessage = "Некорректный номер телефона")]
    public string Phone { get; set; }
}
