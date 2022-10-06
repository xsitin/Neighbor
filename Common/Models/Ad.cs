using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Models;

[JsonSerializable(typeof(Ad))]
public class Ad
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public int Price { get; set; }
    public string OwnerName { get; set; }
    [Required] public string Title { get; set; }
    [Required] public string Description { get; set; }
    public string[]? ImagesIds { get; set; }
    public string Category { get; set; }
    public int? Rating { get; set; }
}
