using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Common.Models;

using MongoDB.Bson.Serialization.Attributes;

public class Account : AccountAuth
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required] public string Name { get; set; }
    [BsonRepresentation(BsonType.String)]
     public Role Role { get; set; }
}
