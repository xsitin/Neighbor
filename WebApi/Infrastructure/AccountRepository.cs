using MongoDB.Driver;

namespace WebApi.Infrastructure;

using Common.Models;

public class AccountRepository : IAccountRepository
{
    private IMongoCollection<Account> Collection { get; set; }


    public AccountRepository(IMongoDatabase db) => Collection = db.GetCollection<Account>("Accounts");

    public async Task<Account?> GetById(string id)
    {
        var result = await Collection.FindAsync(model => model.Id == id);
        return result.FirstOrDefault();
    }

    public async Task<Account?> GetByLoginAsync(string login)
    {
        var result = await Collection.FindAsync(model => model.Login == login);
        return result.FirstOrDefault();
    }

    public async Task Create(Account account) => await Collection.InsertOneAsync(account);

    public async Task Delete(Account account) => await Collection.DeleteOneAsync(x => x.Id == account.Id);

    public async Task Update(Account account)
    {
        var filter = new ExpressionFilterDefinition<Account>(x => x.Login == account.Login);
        var updateDefinition = Builders<Account>
            .Update
            .Set(x => x.Login, account.Login)
            .Set(x => x.Password, account.GetHashedPassword());
        await Collection.UpdateOneAsync(filter, updateDefinition);
    }

    public async Task UpdateRole(string id, Role role) =>
        await Collection.UpdateOneAsync(
            x => x.Id == id,
            Builders<Account>.Update.Set(x => x.Role, role));
}
