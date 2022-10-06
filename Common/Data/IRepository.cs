namespace Common.Data;

using System.Threading.Tasks;

public interface IRepository<T> where T : class
{
    Task<T> GetById(string id);
    Task Create(T entity);
    Task Update(T entity);
    Task Delete(T entity);
}
