namespace Common.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ImageDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; private set; }

    public byte[] Content { get; init; }

    public ImageDocument(byte[] content)
    {
        this.Id = ObjectId.GenerateNewId().ToString();
        this.Content = content;
    }
}
