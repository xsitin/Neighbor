using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Common.Models;

using System;
using MongoDB.Bson.Serialization.Attributes;

public class Account : AccountAuth, ICloneable
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required] public string Name { get; set; }

    [BsonRepresentation(BsonType.String)] public Role Role { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string AvatarId { get; set; }

    public object Clone() => this.MemberwiseClone();
}
