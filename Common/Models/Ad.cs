using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Common.Models
{
    [JsonSerializable(typeof(Ad))]
    public class Ad
    {
        [JsonIgnore] public ObjectId? Id { get; set; }

        public string? SId { get; set; }

        public int Price { get; set; }
        public string OwnerName { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Description { get; set; }
        public string[]? ImagesLinks { get; set; }

        public ObjectId[]? ImagesIds { get; set; }
        public string Category { get; set; }
        public int? Rating { get; set; }
    }
}