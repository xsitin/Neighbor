using Common.Models;

namespace LoadTesting.DataGenerators;

using Token = String;

public interface IAdGenerator
{
    Task<Ad[]> GenerateAd(int count, (AccountRegistration, string)[] usersWithTokens);
    Task<Ad[]> GenerateAd(int count, string category, (AccountRegistration, string)[] usersWithTokens);
}