namespace WebApi.domain;

using Common.Models;

public class RatingCalculator : IRatingCalculator
{
    public int GetRating(Ad ad)
    {
        var rating = 0;
        rating += ad.Description.Length / 10;
        rating += ad.ImagesIds?.Length ?? 0;
        return rating;
    }
}
