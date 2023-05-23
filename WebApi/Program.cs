namespace WebApi;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder => builder.AddEnvironmentVariables(""))
            .ConfigureWebHostDefaults(webBuilder => webBuilder
                .UseKestrel()
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddDebug();
                    builder.AddConsole();
                    builder.AddEventLog();
                })
                .UseStartup<Startup>());
}
