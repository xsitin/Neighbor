using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Board.Infrastructure;
using Common.Models;
using Microsoft.AspNetCore.Components;

namespace Board.Components
{
    public class RegistrationComponent : ComponentBase
    {
        public AccountRegistration AccountData { get; set; } = new();
        [Inject] ILocalStorageService LocalStorage { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] IHttpClientFactory ClientFactory { get; set; }

        public async Task CreateAccount()
        {
            var token = await GetToken();
            if (token is not null)
            {
                await LocalStorage.SetAsync(nameof(SecurityToken), token);
                NavigationManager.NavigateTo("/", true);
            }
        }

        private async Task<SecurityToken> GetToken()
        {
            var client = ClientFactory.CreateClient(Constants.Api);
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(client.BaseAddress, "accounts/registration"));
            request.Content = JsonContent.Create(AccountData);
            var response = await client.SendAsync(request);
            return await response.Content.ReadFromJsonAsync<SecurityToken>();
        }
    }
}