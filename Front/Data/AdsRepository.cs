using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Board.Infrastructure;
using Common.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http.Extensions;

namespace Board.Data;

using System.Globalization;

public class AdsRepository
{
    public AdsRepository(ILocalStorageService storageService, IHttpClientFactory ClientFactory)
    {
        StorageService = storageService;
        this.ClientFactory = ClientFactory;
    }

    private ILocalStorageService StorageService { get; }
    private IHttpClientFactory ClientFactory { get; }

    private async Task<string> GetToken()
    {
        var securityToken = await StorageService.GetAsync<SecurityToken>(nameof(SecurityToken));
        return securityToken.AccessToken;
    }

    public async Task<PaginationInfo<Ad>> GetPopularAsync(int page = 1, int pageSize = 21)
    {
        var client = ClientFactory.CreateClient(Constants.ApiClientName);
        var response =
            await client.GetAsync(new Uri(client.BaseAddress, $"ads/get?page={page}&pageSize={pageSize}"));


        return await response.Content.ReadFromJsonAsync<PaginationInfo<Ad>>();
    }

    public async Task<Ad> GetByIdAsync(string id)
    {
        var client = GetApiClient();
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(client.BaseAddress, "ads/getbyid/" + id));
        var response = await client.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<Ad>();
    }

    public async Task RemoveByIdAsync(string id)
    {
        var client = GetApiClient();
        var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(client.BaseAddress, $"ads/{id}/delete"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());
        await client.SendAsync(request);
    }

    public async Task<PaginationInfo<Ad>> GetUserAdsAsync(string login, int page = 1, int pageSize = 21)
    {
        await StorageService.GetAsync<SecurityToken>(nameof(SecurityToken));
        var client = GetApiClient();
        var queryBuilder = new QueryBuilder
        {
            { nameof(page), page.ToString(CultureInfo.InvariantCulture) },
            { nameof(pageSize), pageSize.ToString(CultureInfo.InvariantCulture) }
        };
        var request = new HttpRequestMessage(HttpMethod.Get,
            new Uri(client.BaseAddress, $"ads/getUserAds/{login}" + queryBuilder.ToQueryString()));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());
        var response = await client.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<PaginationInfo<Ad>>();
    }

    public async Task<PaginationInfo<Ad>> GetWithCategory(string category, int count = 21, int offset = 1)
    {
        var client = ClientFactory.CreateClient(Constants.ApiClientName);
        var queryBuilder = new QueryBuilder(new[]
        {
            new KeyValuePair<string, string>(nameof(category), category),
            new KeyValuePair<string, string>(nameof(count), count.ToString()),
            new KeyValuePair<string, string>(nameof(offset), offset.ToString()),
        });
        var uri = new Uri(client.BaseAddress, "ads/categories" + queryBuilder.ToQueryString());
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await client.SendAsync(request);
        var ads = await response.Content.ReadFromJsonAsync<PaginationInfo<Ad>>();

        return ads;
    }

    public async Task<PaginationInfo<Ad>> SearchWithTitle(string query, int count = 21, int page = 1)
    {
        var client = ClientFactory.CreateClient(Constants.ApiClientName);
        var queryBuilder = new QueryBuilder(new[]
        {
            new KeyValuePair<string, string>(nameof(query), query),
            new KeyValuePair<string, string>(nameof(count), count.ToString()),
            new KeyValuePair<string, string>(nameof(page), page.ToString()),
        });
        var uri = new Uri(client.BaseAddress, "ads/search" + queryBuilder.ToQueryString());
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await client.SendAsync(request);
        var ads = await response.Content.ReadFromJsonAsync<PaginationInfo<Ad>>();

        return ads;
    }

    public async Task Update(Ad ad, IBrowserFile[] images)
    {
        await StorageService.GetAsync<SecurityToken>(nameof(SecurityToken));
        var client = GetApiClient();
        var request = new HttpRequestMessage(HttpMethod.Patch, new Uri(client.BaseAddress, "ads/update"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(JsonSerializer.Serialize(ad)), nameof(Ad));
        foreach (var image in images)
            content.Add(new StreamContent(image.OpenReadStream(int.MaxValue)), image.Name, image.Name);
        request.Content = content;
        await client.SendAsync(request);
    }

    public async Task Add(Ad ad, IBrowserFile[] images)
    {
        var client = GetApiClient();
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(client.BaseAddress, "ads/add"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(JsonSerializer.Serialize(ad)), nameof(Ad));
        foreach (var image in images)
            content.Add(new StreamContent(image.OpenReadStream(int.MaxValue)), image.Name, image.Name);
        request.Content = content;
        await client.SendAsync(request);
    }

    private HttpClient GetApiClient() => ClientFactory.CreateClient(Constants.ApiClientName);
}
