using System.Net.Http.Json;
using Common.Models;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;

namespace LoadTesting;

public class Steps
{
    public Steps(Config config)
    {
        var result = new HttpClient().GetAsync($"{config.baseUrl}ads/get").Result;
        var ads = result.Content.ReadFromJsonAsync<Ad[]>().Result;

        GetPopularJson = Step.Create("GetPopularJson", HttpClientFactory.Create("GetPopularJson"), async context =>
        {
            var result = await context.Client.GetAsync($"{config.baseUrl}ads/get");
            var length = 0;
            if (result.RequestMessage?.Content != null)
                length = (int) result.RequestMessage.Content.ReadAsStream().Length;
            length += (int) result.Content.ReadAsStream().Length;
            return Response.Ok(sizeBytes: length, statusCode: (int) result.StatusCode);
        }, TimeSpan.FromSeconds(5));

        GetFirstImages = Step.Create("GetPopularImages", HttpClientFactory.Create(), async context =>
        {
            var length = 0;
            if (ads is {Length: > 0})
            {
                var tasks = new HttpResponseMessage[ads.Length];
                for (var i = 0; i < ads.Length; i++)
                {
                    var ad = ads[i];

                    tasks[i] = await context.Client.GetAsync(config.baseUrl + "img/get/"+ad.ImagesIds[0]);
                }

                length += tasks.Sum(x => (int) x.Content.ReadAsStream().Length);
            }

            return Response.Ok(sizeBytes: length);
        }, TimeSpan.FromSeconds(5));
    }

    public IStep GetPopularJson { get; set; }

    public IStep GetFirstImages { get; set; }
}
