namespace Board.Data;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Models;
using Infrastructure;
using Microsoft.AspNetCore.Components.Forms;

public class AccountRepository
{
    public AccountRepository(IHttpClientFactory clientFactory) => ClientFactory = clientFactory;

    private IHttpClientFactory ClientFactory { get; set; }


    public async Task<SecurityToken> Register(AccountRegistration account, IBrowserFile avatar)
    {
        var client = ClientFactory.CreateClient(Constants.ApiClientName);
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(client.BaseAddress!, "accounts/registration"));
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(JsonSerializer.Serialize(account)), nameof(AccountRegistration));
        content.Add(new StreamContent(avatar.OpenReadStream()), nameof(avatar), avatar.Name);
        request.Content = content;
        var response = await client.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<SecurityToken>();
    }
}
