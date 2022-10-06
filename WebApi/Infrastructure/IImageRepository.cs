namespace WebApi.Infrastructure;

using Common.Data;
using Common.Models;
using MongoDB.Bson;

public interface IImageRepository : IRepository<ImageDocument>

{
    public Task DeleteManyAsync(IEnumerable<string> ids);
    public Task DeleteManyAsync(IEnumerable<ObjectId> ids);
    public byte[] GetContent(string id);
    public Task<ImageDocument> Add(IFormFile file);
    IEnumerable<Task<ImageDocument>> AddAll(IFormFileCollection files);
}
