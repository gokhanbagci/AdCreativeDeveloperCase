using AdCreativeDeveloperCase.Application.Business;
using AdCreativeDeveloperCase.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
internal class Program
{
    private static CancellationTokenSource _cts = new ();
    static async Task<int> Main(string[] args)
    {
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("Cancel event triggered");
            _cts.Cancel();
            eventArgs.Cancel = true;
        };

        var _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("DownloadSettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var builder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient();
                services.AddTransient<ConsoleApplication>();

                var downloadSettings = new DownloadSettings();
                _configuration.Bind(downloadSettings);
                services.AddSingleton(downloadSettings);

            }).UseConsoleLifetime();

        var host = builder.Build();

        using var serviceScope = host.Services.CreateScope();
        try
        {
            var app = serviceScope.ServiceProvider.GetRequiredService<ConsoleApplication>();
            await app.Run(_cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error:" + ex.Message);
        }
        finally
        {
            _cts.Dispose();
        }
        return 0;
    }
}