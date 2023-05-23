namespace WebApi.Infrastructure;

using Common.Models;
using MongoDB.Bson;
using MongoDB.Driver;

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
        var imageDocument = Collection.Find(builder.Eq<string>(x => x.Id, id)).First();
        return imageDocument.Content;
    }


    public async Task<ImageDocument> Add(IFormFile image)
    {
        var imageStream = image.OpenReadStream();
        var content = new byte[imageStream.Length];
        await imageStream.ReadAsync(content);
        imageStream.Close();
        var doc = new ImageDocument(content);
        await Collection?.InsertOneAsync(doc)!;
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
