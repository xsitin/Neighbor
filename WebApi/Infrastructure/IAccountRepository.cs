using Common.Models;
using MongoDB.Bson;

namespace WebApi.Infrastructure;

using Common.Data;

public interface IAccountRepository : IRepository<Account>
{
    public Task<Account?> GetByLoginAsync(string login);
    public Task UpdateRole(string id, Role role);
}
