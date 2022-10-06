using System.Linq;
using MongoDB.Bson;

namespace Common.Models;

using MongoDB.Bson.Serialization.Attributes;

public class ImageDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; private set; }

    public string Path { get; private set; }


    public static ImageDocument Create()
    {
        var doc = new ImageDocument() {Id = ObjectId.GenerateNewId().ToString()};
        doc.Path = System.IO.Path.Combine(doc.Id.Chunk(doc.Id.Length / 2).Select(x => new string(x)).ToArray());
        return doc;
    }
}