using System;
using System.Net.Http;
using System.Threading.Tasks;
using Board.Components;
using Board.Infrastructure;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Board
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient(Constants.Api,
                client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5000");
                    client.MaxResponseContentBufferSize = 2147483647;
                });
            builder.Services.AddScoped<AdsRepository>();
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddLogging();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
            await builder.Build().RunAsync();
        }
    }
}