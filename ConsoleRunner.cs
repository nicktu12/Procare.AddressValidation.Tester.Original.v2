//-----------------------------------------------------------------------
// <copyright file="ConsoleRunner.cs" company="Procare Software, LLC">
//     Copyright © 2021-2025 Procare Software, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Procare.AddressValidation.Tester;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal sealed class ConsoleRunner : IHostedService
{
    private readonly ILogger<ConsoleRunner> logger;
    private readonly IHostApplicationLifetime lifetime;
    private readonly AddressValidationService addressValidationService;
    private readonly string[] args;

    internal ConsoleRunner(ILogger<ConsoleRunner> logger, IHostApplicationLifetime lifetime, AddressValidationService addressValidationService, string[] args)
    {
        this.logger = logger;
        this.lifetime = lifetime;
        this.addressValidationService = addressValidationService;
        this.args = args;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Starting.");
        this.lifetime.StopApplication();
        return Task.CompletedTask;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Main method should not leak any exceptions.")]
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            List<AddressValidationRequest> requests = this.ParseArguments();
            if (requests.Count == 0)
            {
                requests.Add(new AddressValidationRequest() { Line1 = "1125 17th St Ste 1800", City = "Denver", StateCode = "CO", ZipCodeLeading5 = "80202" });
            }

            foreach (AddressValidationRequest request in requests)
            {
                this.logger.LogInformation("Validating address {AddressValidationRequest}.", request);
                string response = await this.addressValidationService.GetAddressesAsync(request, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Received address validation response {AddressValidationResponse}.", response);
            }
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "An unexpected error occurred.");
        }
        finally
        {
            this.logger.LogInformation("Done.");
        }
    }

    private List<AddressValidationRequest> ParseArguments()
    {
        if (this.args == null || this.args.Length == 0)
        {
            return [];
        }

        List<AddressValidationRequest> requests = new(this.args.Length);
        for (int i = 0; i < this.args.Length; i++)
        {
            string? arg = this.args[i];

            if (string.IsNullOrEmpty(arg))
            {
                continue;
            }

            try
            {
                AddressValidationRequest request = JsonSerializer.Deserialize<AddressValidationRequest>(arg);
                requests.Add(request);
            }
            catch (Exception e)
            {
                throw new JsonException(string.Format(CultureInfo.CurrentCulture, "An error occurred parsing argument {0}.", i), e);
            }
        }

        return requests;
    }
}
