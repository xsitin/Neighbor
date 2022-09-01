using System.Threading.Tasks;
using Common.Models;
using MongoDB.Bson;

namespace WebApi.Infrastructure
{
    public interface IAccountRepository
    {
        public Task<Account?> GetByIdAsync(ObjectId id);
        public Task<Account?> GetByLoginAsync(string login);
        public Task CreateUserAsync(Account account);
        public Task RemoveById(ObjectId id);
        public Task Update(Account account);
        public Task UpdateRole(ObjectId id, string role);
    }
}