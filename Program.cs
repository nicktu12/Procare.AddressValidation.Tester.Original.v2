//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Procare Software, LLC">
//     Copyright © 2021-2025 Procare Software, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Procare.AddressValidation.Tester;

using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddLogging((logger) =>
                {
                    logger.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss.fff ";
                    });
                });

                services.AddHttpClient(AddressValidationService.HttpClientName, client =>
                {
                    client.BaseAddress = new("https://addresses.dev-procarepay.com");
                });

                services.AddTransient<AddressValidationService>();
                services.AddHostedService((services) => new ConsoleRunner(
                    services.GetRequiredKeyedService<ILogger<ConsoleRunner>>(null),
                    services.GetRequiredService<IHostApplicationLifetime>(),
                    services.GetRequiredKeyedService<AddressValidationService>(null),
                    args));
            });

        await hostBuilder.RunConsoleAsync().ConfigureAwait(false);
    }
}
