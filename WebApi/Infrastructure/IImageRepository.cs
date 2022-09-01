using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace WebApi.Infrastructure
{
    public interface IImageRepository
    {
        public byte[] Get(string id);
        public Task Add(ImageDocument imageDocument, IFormFile img);
        public Task<ImageDocument> Add(IFormFile file);
        public Task DeleteAllAsync(IEnumerable<ObjectId>? ids);
        public IEnumerable<Task<ImageDocument>> AddAll(IFormFileCollection files);
    }
}