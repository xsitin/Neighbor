using System.Linq;
using MongoDB.Bson;

namespace BoardCommon.Models
{
    public class ImageDocument
    {
        public string Path;
        public ObjectId Id;

        public static ImageDocument Create()
        {
            var doc = new ImageDocument()
            {
                Id = ObjectId.GenerateNewId()
            };
            var sid = doc.Id.ToString();
            doc.Path = System.IO.Path.Combine(sid.Chunk(sid.Length / 2).Select(x => new string(x)).ToArray());
            return doc;
        }
    }
}