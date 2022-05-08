using System.Threading.Tasks;
using api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Account = BoardCommon.Models.Account;

namespace api.Infrastructure
{
    public class AccountRepository:IAccountRepository
    {
        private IMongoCollection<Account> Collection { get; set; }


        public AccountRepository(IMongoDatabase db) => Collection = db.GetCollection<Account>("Accounts");

        public async Task<Account?> GetByIdAsync(ObjectId id)
        {
            var result = await Collection.FindAsync(model => model.Id == id);
            return result.FirstOrDefault();
        }

        public async Task<Account?> GetByLoginAsync(string login)
        {
            var result = await Collection.FindAsync(model => model.Login == login);
            return result.FirstOrDefault();
        }

        public async Task CreateUserAsync(Account account) => await Collection.InsertOneAsync(account);

        public async Task RemoveById(ObjectId id) => await Collection.DeleteOneAsync(x => x.Id == id);

        public async Task Update(Account account)
        {
            var filter = new ExpressionFilterDefinition<Account>(x => x.Id == account.Id);
            var updateDefinition = Builders<Account>
                .Update
                .Set(x => x.Login, account.Login)
                .Set(x => x.Password, account.GetHashedPassword());
            await Collection.UpdateOneAsync(filter, updateDefinition);
        }

        public async Task UpdateRole(ObjectId id, string role) =>
            await Collection.UpdateOneAsync(
                x => x.Id == id,
                Builders<Account>.Update.Set(x => x.Role, role));
    }
}