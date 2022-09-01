using System.Net.Http.Json;
using Common.Models;
using Faker;

namespace LoadTesting.DataGenerators;

public class UserGenerator
{
    private readonly string baseUrl;

    public UserGenerator(string baseUrl)
    {
        this.baseUrl = baseUrl;
    }

    AccountRegistration[] CreateAccounts(int count)
    {
        var result = new AccountRegistration[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = new AccountRegistration()
            {
                Login = StringFaker.AlphaNumeric(10), Password = StringFaker.AlphaNumeric(10),
                Name = NameFaker.Name()
            };
        }

        return result;
    }

    void AddAccounts(AccountRegistration[] accounts)
    {
        var requests = accounts.Select(account =>
        {
            var request =
                new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "accounts/registration"));
            request.Content = JsonContent.Create(account);
            return request;
        });

        const int chunkSize = 100;
        var clients = new HttpClient[chunkSize];
        for (var i = 0; i < 100; i++) clients[i] = new HttpClient();

        foreach (var requestsChunk in requests.Chunk(chunkSize))
        {
            var results = new Task[requestsChunk.Length];
            for (var i = 0; i < requestsChunk.Length; i++)
                results[i] = clients[i].SendAsync(requestsChunk[i]);

            Task.WaitAll(results);
        }

        foreach (var httpClient in clients) httpClient.Dispose();
    }

    public AccountRegistration[] GetAccounts(int count)
    {
        var accounts = CreateAccounts(count);
        AddAccounts(accounts);
        return accounts;
    }

    public async Task<string[]> GetTokens(AccountRegistration[] accounts)
    {
        var client = new HttpClient();
        var result = new string[accounts.Length];
        for (var i = 0; i < accounts.Length; i++)
        {
            var account = accounts[i];
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "accounts/login"));
            request.Content = JsonContent.Create(account);
            var response = await client.SendAsync(request);
            var token = response.Content.ReadFromJsonAsync<SecurityToken>().Result.AccessToken;
            result[i] = token;
        }

        return result;
    }
}