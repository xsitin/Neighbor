namespace Common.Data;

using System.Threading.Tasks;
using Models;

public interface IAdRepository:IRepository<Ad>
{
    Task<PaginationInfo<Ad>> GetPage(AdRequest request);
}
