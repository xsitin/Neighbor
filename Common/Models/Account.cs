using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Common.Models
{
    public class Account : AccountAuth
    {
        [Required]
        public string Name { get; set; }
        public string? Role { get; set; }
        public ObjectId? Id { get; set; }
    }
}