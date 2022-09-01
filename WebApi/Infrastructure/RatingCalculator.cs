using Common.Models;

namespace WebApi.Infrastructure
{
    public class RatingCalculator : IRatingCalculator
    {
        public int GetRating(Ad ad)
        {
            var rating = 0;
            rating += ad.Description.Length / 10;
            rating += ad.ImagesLinks?.Length ?? 0;
            return rating;
        }
    }

    public interface IRatingCalculator
    {
        public int GetRating(Ad ad);
    }
}