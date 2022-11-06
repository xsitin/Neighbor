namespace Board.Data;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using Infrastructure;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

public class AccountRepository
{
    public AccountRepository(IHttpClientFactory clientFactory, ILocalStorageService storageService)
    {
        ClientFactory = clientFactory;
        StorageService = storageService;
    }

    private ILocalStorageService StorageService { get; }

    private IHttpClientFactory ClientFactory { get; set; }

    private async Task<string> GetToken()
    {
        var securityToken = await StorageService.GetAsync<SecurityToken>(nameof(SecurityToken));
        return securityToken.AccessToken;
    }

    public async Task<SecurityToken> Register(AccountRegistration account, IBrowserFile avatar)
    {
        var client = ClientFactory.CreateClient(Constants.ApiClientName);
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(client.BaseAddress!, "accounts/registration"));
        var content = new MultipartFormDataContent();
        content.Add(JsonContent.Create(account), nameof(AccountRegistration));
        content.Add(new StreamContent(avatar.OpenReadStream()), nameof(avatar), avatar.Name);
        request.Content = content;
        var response = await client.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<SecurityToken>();
    }

    public async Task<AccountViewModel> GetAccount(string login)
    {
        var client = ClientFactory.CreateClient(Constants.ApiClientName);
        return await client.GetFromJsonAsync<AccountViewModel>($"accounts/{login}");
    }

    public async Task<Account> GetFullAccount(string login)
    {
        var client = ClientFactory.CreateClient(Constants.ApiClientName);
        var request = new HttpRequestMessage(HttpMethod.Get, $"accounts/{login}/full");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Account>();
    }

    public async Task Update(string login, JsonPatchDocument patchDocument)
    {
        var client = ClientFactory.CreateClient(Constants.ApiClientName);
        var request = new HttpRequestMessage(HttpMethod.Patch, $"accounts/{login}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());
        var content = new StringContent(JsonConvert.SerializeObject(patchDocument), Encoding.UTF8,
            "application/json-patch+json");
        request.Content = content;
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
