using Common.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace WebApi.Infrastructure;

using Common.Data;
using domain;

public class AdRepository : IAdRepository
{
    private IMongoCollection<Ad> Collection { get; }
    private IImageRepository ImageRepository { get; }
    private IRatingCalculator RatingCalculator { get; }


    public AdRepository(IMongoDatabase db, IRatingCalculator ratingCalculator, IImageRepository imageRepository)
    {
        RatingCalculator = ratingCalculator;
        ImageRepository = imageRepository;
        Collection = db.GetCollection<Ad>("Ads");
    }

    private static FilterDefinition<Ad> BuildFilter(AdRequest request, out SortDefinition<Ad>? sort)
    {
        var builder = new FilterDefinitionBuilder<Ad>();
        var filters = new List<FilterDefinition<Ad>>();
        sort = null;
        if (request.Types.HasFlag(DataRequestTypes.Popular))
        {
            sort = Builders<Ad>.Sort.Descending(x => x.Rating);
        }

        if (request.Types.HasFlag(DataRequestTypes.Search))
        {
            filters.Add(new ExpressionFilterDefinition<Ad>(x =>
                x.Title.ToLowerInvariant().Contains(request.Query) ||
                x.Description.ToLowerInvariant().Contains(request.Query) ||
                x.OwnerName.ToLowerInvariant().Contains(request.Query)));
        }

        if (request.Types.HasFlag(DataRequestTypes.FromUser))
        {
            filters.Add(builder.Eq(x => x.OwnerName, request.Username));
        }

        if (request.Types.HasFlag(DataRequestTypes.FromCategory))
        {
            filters.Add(builder.Eq(x => x.Category, request.Category));
        }

        var filter = filters.Count > 0 ? builder.And(filters) : FilterDefinition<Ad>.Empty;
        return filter;
    }

    public async Task<Ad?> GetById(string id)
    {
        var result = await Collection.FindOneAndUpdateAsync(model => model.Id == id,
            new UpdateDefinitionBuilder<Ad>().Inc(x => x.Rating, 1));
        return result;
    }

    public async Task Create(Ad entity)
    {
        entity.Id = ObjectId.GenerateNewId().ToString();
        entity.Rating = RatingCalculator.GetRating(entity);
        await Collection.InsertOneAsync(entity);
    }

    public async Task Update(Ad entity)
    {
        var saved = await GetById(entity.Id);
        if (saved.ImagesIds != null)
            await ImageRepository.DeleteManyAsync(saved.ImagesIds);
        entity.Rating = saved.Rating;
        await Collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
    }

    public async Task Delete(Ad entity)
    {
        var ad = await Collection.FindOneAndDeleteAsync(x => x.Id == entity.Id);
        await ImageRepository.DeleteManyAsync(ad.ImagesIds);
    }


    public async Task<PaginationInfo<Ad>> GetPage(AdRequest request)
    {
        var filter = BuildFilter(request, out var sort);

        var cursor = await Collection.FindAsync(filter,
            new FindOptions<Ad>()
            {
                Sort = sort,
                Limit = request.PaginationInfo.PageSize,
                Skip = (request.PaginationInfo.Page - 1) * request.PaginationInfo.PageSize
            });
        var count = await Collection.CountDocumentsAsync(filter);
        var paginationInfo = new PaginationInfo<Ad>()
        {
            Items = cursor.ToList(),
            Page = request.PaginationInfo.Page,
            PageSize = request.PaginationInfo.PageSize,
            ItemsCount = (int)count,
            PageCount = (int)Math.Ceiling(((double)count) / request.PaginationInfo.PageSize)
        };
        return paginationInfo;
    }
}
