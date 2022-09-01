using Common.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace WebApi.Infrastructure
{
    public class ImageRepository : IImageRepository
    {
        private readonly IMongoCollection<ImageDocument>? _imagesCollection;
        private readonly string _pathToFilesDirectory;

        public ImageRepository(IMongoCollection<ImageDocument>? imagesCollection, string pathToFilesDirectory)
        {
            this._imagesCollection = imagesCollection;
            this._pathToFilesDirectory = pathToFilesDirectory;
        }

        public byte[] Get(string id)
        {
            var builder = new FilterDefinitionBuilder<ImageDocument>();
            var imageDocument = _imagesCollection.Find(builder.Eq(x => x.Id, new ObjectId(id))).First();
            return File.ReadAllBytes(Path.Combine(_pathToFilesDirectory, imageDocument.Path));
        }

        public async Task Add(ImageDocument imageDocument, IFormFile img)
        {
            await _imagesCollection?.InsertOneAsync(imageDocument)!;
            Directory.CreateDirectory(Path.Combine(_pathToFilesDirectory,
                imageDocument.Path.Split(Path.DirectorySeparatorChar)[0]));
            var file = File.OpenWrite(Path.Combine(_pathToFilesDirectory, imageDocument.Path));
            var imageStream = img.OpenReadStream();
            await imageStream.CopyToAsync(file);
            file.Close();
            imageStream.Close();
        }

        public async Task<ImageDocument> Add(IFormFile file)
        {
            var doc = ImageDocument.Create();
            await Add(doc, file);
            return doc;
        }

        public async Task DeleteAllAsync(IEnumerable<ObjectId>? ids)
        {
            ids = ids.ToArray();

            foreach (var path in ids.Select(GetPathTo))
                File.Delete(path);


            await _imagesCollection?.DeleteManyAsync(Builders<ImageDocument>.Filter.In(x => x.Id, ids))!;
        }

        public IEnumerable<Task<ImageDocument>> AddAll(IFormFileCollection files) =>
            files.Select(async formFile => await Add(formFile));

        private string GetPathTo(ObjectId imageId)
        {
            var sid = imageId.ToString();
            var relativePath = Path.Combine(sid.Chunk(sid.Length / 2).Select(x => new string(x)).ToArray());
            var absolutePath = Path.Combine(_pathToFilesDirectory, relativePath);
            return absolutePath;
        }
    }
}