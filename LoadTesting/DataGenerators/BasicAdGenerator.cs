using System.Net.Http.Headers;
using System.Text.Json;
using Common.Enums;
using Common.Models;
using Faker;
using MongoDB.Bson;

namespace LoadTesting.DataGenerators;

using Id = String;
using Token = String;

public class BasicAdGenerator : IAdGenerator
{
    private IImagesGenerator ImagesGenerator { get; set; }
    private readonly Config config;

    public BasicAdGenerator(IImagesGenerator imagesGenerator, Config config)
    {
        ImagesGenerator = imagesGenerator;
        this.config = config;
    }

    public async Task<Ad[]> GenerateAd(int count, (AccountRegistration, string)[] usersWithTokens)
    {
        return await GenerateAd(count, Categories.Flat, usersWithTokens);
    }

    public async Task<Ad[]> GenerateAd(int count, string category, (AccountRegistration, string)[] usersWithTokens)
    {
        var result = new (Ad, Token, byte[][])[count];
        for (var i = 0; i < count; i++)
        {
            var images = await ImagesGenerator.GenerateImages(3);
            var user = usersWithTokens[Random.Shared.Next(0, usersWithTokens.Length)];
            var ad = new Ad
            {
                Category = category,
                Description = TextFaker.Sentences(3),
                Id = ObjectId.GenerateNewId().ToString(),
                Price = Random.Shared.Next(),
                OwnerName = user.Item1.Name,
                Rating = Random.Shared.Next(),
                Title = CompanyFaker.Name()
            };
            ad.Id = ad.Id.ToString();
            result[i] = (ad, user.Item2, images);
        }

        await LoadAds(result);

        return result.Select(x => x.Item1).ToArray();
    }

    private async Task LoadAds((Ad, Token, byte[][])[] ads)
    {
        var client = new HttpClient();
        for (var i = 0; i < ads.Length; i++)
        {
            var ad = ads[i];
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(config.baseUrl), "ads/add"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ad.Item2);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(JsonSerializer.Serialize(ad.Item1)), nameof(Ad));
            foreach (var image in ad.Item3)
                content.Add(new ByteArrayContent(image), StringFaker.AlphaNumeric(10),
                    StringFaker.AlphaNumeric(10));
            request.Content = content;
            await client.SendAsync(request);
        }
    }
}
