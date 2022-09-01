using LoadTesting;
using LoadTesting.DataGenerators;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;


var config = new Config
{
    baseUrl = "http://localhost:5000/",
    platform = Platforms.Dotnet,
    nodejsDbPath = @"C:\Users\xsitin\Desktop\board-WebApi-master\board-database"
};

for (var i = 0; i < 30; i++)
{
    var steps = new Steps(config);
    var loadHomeJson = steps.GetPopularJson;
    var loadHomeImages = steps.GetFirstImages;
    var scenario = ScenarioBuilder
        .CreateScenario("LoadHome", loadHomeJson, loadHomeImages)
        .WithLoadSimulations(LoadSimulation.NewInjectPerSec((i + 1) * 10, TimeSpan.FromSeconds(20)));

    NBomberRunner.RegisterScenarios(scenario).Run();
    Thread.Sleep(10_000);
}