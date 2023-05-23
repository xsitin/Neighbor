namespace Board;

using System;
using System.Threading.Tasks;
using Data;
using Infrastructure;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.Services.AddHttpClient(Constants.ApiClientName,
            client =>
            {
                client.BaseAddress = new Uri(Constants.ApiPath);
                client.MaxResponseContentBufferSize = 2147483647;
            });
        builder.Services.AddScoped<AdsRepository>();
        builder.Services.AddOptions();
        builder.Services.AddAuthorizationCore();
        builder.Services.AddLogging();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
        builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
        builder.Services.AddSingleton(new ImageHelper(Constants.ApiPath));
        builder.Services.AddScoped<AccountRepository>();
        await builder.Build().RunAsync();
    }
}
