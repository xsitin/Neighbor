using System.ComponentModel.DataAnnotations;

namespace BoardCommon.Models
{
    public class AccountRegistration:AccountAuth
    {
        [Required]
        public string Name { get; set; }
    }
}