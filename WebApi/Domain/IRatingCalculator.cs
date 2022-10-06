namespace WebApi.domain;

using Common.Models;

public interface IRatingCalculator
{
    public int GetRating(Ad ad);
}
