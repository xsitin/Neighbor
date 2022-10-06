using Common.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace WebApi.Infrastructure;

public class ImageRepository : IImageRepository
{
    private IMongoCollection<ImageDocument>? Collection { get; init; }
    private readonly string pathToFilesDirectory;

    public ImageRepository(IMongoCollection<ImageDocument>? collection, string pathToFilesDirectory)
    {
        Collection = collection;
        this.pathToFilesDirectory = pathToFilesDirectory;
    }

    public byte[] GetContent(string id)
    {
        var builder = new FilterDefinitionBuilder<ImageDocument>();
        var imageDocument = Collection.Find(builder.Eq<string>(x => x.Id,id)).First();
        return File.ReadAllBytes(Path.Combine(pathToFilesDirectory, imageDocument.Path));
    }

    private async Task Add(ImageDocument imageDocument, IFormFile img)
    {
        await Collection?.InsertOneAsync(imageDocument)!;
        Directory.CreateDirectory(Path.Combine(pathToFilesDirectory,
            imageDocument.Path.Split(Path.DirectorySeparatorChar)[0]));
        var file = File.OpenWrite(Path.Combine(pathToFilesDirectory, imageDocument.Path));
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

    public IEnumerable<Task<ImageDocument>> AddAll(IFormFileCollection files) =>
        files.Select(async formFile => await Add(formFile));

    public Task<ImageDocument> GetById(string id) => Task.FromResult(Collection.Find(x => x.Id == id).First());

    public async Task Create(ImageDocument entity) => await Collection.InsertOneAsync(entity);

    public Task Update(ImageDocument entity) =>
        Task.FromResult(Collection.FindOneAndReplace(x => x.Id == entity.Id, entity));

    public Task Delete(ImageDocument entity) => Collection.DeleteOneAsync(x => x.Id == entity.Id);

    public Task DeleteManyAsync(IEnumerable<string> ids)
    {
        var idSet = ids.ToHashSet();
        return Collection.DeleteManyAsync(x => idSet.Contains(x.Id));
    }

    public Task DeleteManyAsync(IEnumerable<ObjectId> ids)
    {
        var idSet = ids.ToHashSet();
        return Collection.DeleteManyAsync(x => idSet.Contains(ObjectId.Parse(x.Id)));
    }
}
