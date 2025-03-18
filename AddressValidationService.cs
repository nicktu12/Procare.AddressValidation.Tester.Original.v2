﻿//-----------------------------------------------------------------------
// <copyright file="AddressValidationService.cs" company="Procare Software, LLC">
//     Copyright © 2021-2025 Procare Software, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Procare.AddressValidation.Tester;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "This is instantiated via DI.")]
internal sealed class AddressValidationService
{
    internal const string HttpClientName = "AddressValidationHttpClient";

    private const int MaxRetries = 3;

    private const int TimeoutMilliseconds = 750;

    private readonly IHttpClientFactory httpClientFactory;

    public AddressValidationService(IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory, nameof(httpClientFactory));

        this.httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetAddressesAsync(AddressValidationRequest request, CancellationToken token = default)
    {
        int retryCount = 0;
        while (true)
        {
            try
            {
                using var timeoutCts = new CancellationTokenSource(TimeoutMilliseconds);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, token);
                using HttpClient httpClient = this.httpClientFactory.CreateClient(HttpClientName);
                using HttpRequestMessage httpRequest = request.ToHttpRequest();
                using HttpResponseMessage response = await httpClient.SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    linkedCts.Token).ConfigureAwait(false);

                if ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600)
                {
                    if (retryCount < MaxRetries)
                    {
                        retryCount++;
                        Console.WriteLine($"Received HTTP {(int)response.StatusCode} error. Retry attempt {retryCount} of {MaxRetries}");
                        await Task.Delay(100 * (int)Math.Pow(2, retryCount - 1), token).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"Max retries ({MaxRetries}) reached for HTTP {(int)response.StatusCode} error. Throwing exception.");
                        response.EnsureSuccessStatusCode();
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Received non-5xx error ({(int)response.StatusCode}). Throwing immediately without retry.");
                    response.EnsureSuccessStatusCode();
                }

                return await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (retryCount < MaxRetries)
            {
                retryCount++;
                await Task.Delay(100 * (int)Math.Pow(2, retryCount - 1), token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Max retries ({MaxRetries}) reached for timeout. Throwing exception.");
                throw;
            }
            catch (HttpRequestException) when (retryCount < MaxRetries)
            {
                retryCount++;
                await Task.Delay(100 * (int)Math.Pow(2, retryCount - 1), token).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }
    }
}
