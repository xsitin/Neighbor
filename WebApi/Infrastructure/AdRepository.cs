using Common.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace WebApi.Infrastructure
{
    public class AdRepository
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

        public async Task<Ad> GetByIdAsync(ObjectId id)
        {
            var result = await Collection.FindOneAndUpdateAsync(model => model.Id == id,
                new UpdateDefinitionBuilder<Ad>().Inc(x => x.Rating, 1));
            result.SId = result.Id.ToString();
            result.Id = null;
            return result;
        }


        public async Task<Ad[]> GetByUserNameAsync(string name)
        {
            var cursor = await Collection.FindAsync(x => x.OwnerName == name);
            cursor.MoveNext();
            var ads = cursor.Current.ToArray();
            foreach (var ad in ads) ad.SId = ad.Id.ToString();
            return ads;
        }

        public async Task<PaginationInfo<Ad>> GetRecommendedAsync(int page, int pageSize)
        {
            var filter = Builders<Ad>.Filter.Empty;
            var cursor = await Collection.FindAsync(filter,
                new FindOptions<Ad>()
                {
                    Limit = pageSize,
                    Skip = (page - 1) * pageSize,
                    Sort = Builders<Ad>.Sort.Descending(ad => ad.Rating)
                });
            var count = (int) await Collection.CountDocumentsAsync(filter);
            var paginationInfo = new PaginationInfo<Ad>
            {
                ItemsCount = count,
                Page = page,
                PageCount = (int) Math.Ceiling(((double)count) / pageSize),
                PageSize = pageSize,
                Items = cursor.ToEnumerable().ToList()
            };

            return paginationInfo;
        }

        public async Task AddAsync(Ad ad, IFormFileCollection images)
        {
            var imgDocs = ImageRepository.AddAll(images).ToArray();
            ad.Id = ObjectId.GenerateNewId();
            ad.Rating = RatingCalculator.GetRating(ad);
            ad.ImagesLinks = imgDocs.Select(x => "img/get/" + x.Result.Id).ToArray();
            ad.ImagesIds = imgDocs.Select(x => x.Result.Id).ToArray();
            await Collection.InsertOneAsync(ad);
        }

        public async Task UpdateAsync(Ad ad, IFormFileCollection images)
        {
            var previous = await GetByIdAsync(new ObjectId(ad.SId));
            var imgDocs = ImageRepository.AddAll(images).Select(x => x.Result).ToArray();
            var imagesId = imgDocs.Select(x => x.Id);
            var imagesLinks = imgDocs.Select(x => "img/get/" + x.Id).ToArray();
            await ImageRepository.DeleteAllAsync(previous.ImagesIds);
            await Collection.UpdateOneAsync(Builders<Ad>.Filter.Where(x => x.Id == ObjectId.Parse(ad.SId)),
                Builders<Ad>.Update
                    .Set(x => x.Category, ad.Category)
                    .Set(x => x.Description, ad.Description)
                    .Set(x => x.Title, ad.Title)
                    .Set(x => x.Price, ad.Price)
                    .Set(x => x.ImagesIds, imagesId)
                    .Set(x => x.ImagesLinks, imagesLinks)
            );
        }

        public async Task DeleteAsync(ObjectId id)
        {
            var ad = await Collection.FindOneAndDeleteAsync(x => x.Id == id);
            await ImageRepository.DeleteAllAsync(ad.ImagesIds);
        }

        public async Task<Ad[]> GetSimilar(string searchText, int page = 1, int pageSize = 21)
        {
            searchText = searchText.ToLowerInvariant();
            var filter = new ExpressionFilterDefinition<Ad>(x =>
                x.Title.ToLowerInvariant().Contains(searchText) ||
                x.Description.ToLowerInvariant().Contains(searchText) ||
                x.OwnerName.ToLowerInvariant().Contains(searchText));
            var cursor = await Collection.FindAsync(filter,
                new FindOptions<Ad>() {Limit = pageSize, Skip = (page - 1) * pageSize});
            await cursor.MoveNextAsync();
            return cursor.Current.ToArray();
        }

        public async Task<Ad[]> GetWithCategory(string category, int page = 1, int pageSize = 21)
        {
            var filter = new ExpressionFilterDefinition<Ad>(x => x.Category == category);
            var cursor = await Collection.FindAsync(filter,
                new FindOptions<Ad> {Limit = pageSize, Skip = (page - 1) * pageSize});
            await cursor.MoveNextAsync();
            return cursor.Current.Select(x =>
            {
                x.SId = x.Id.ToString();
                return x;
            }).ToArray();
        }
    }
}