//-----------------------------------------------------------------------
// <copyright file="AddressValidationServiceTests.cs" company="Procare Software, LLC">
//     Copyright © 2021-2025 Procare Software, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Procare.AddressValidation.Tester.Tests;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

[CollectionDefinition("AddressValidation")]
public class AddressValidationCollection : ICollectionFixture<AddressValidationFixture>
{
}

public class AddressValidationFixture : IDisposable
{
    private const string BaseAddress = "https://api.test.com/";
    private bool disposed;

    public AddressValidationFixture()
    {
        this.HttpMessageHandlerMock = new Mock<HttpMessageHandler>();
        this.HttpMessageHandlerMock.CallBase = false;
    }

    public Mock<HttpMessageHandler> HttpMessageHandlerMock { get; }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            this.disposed = true;
        }
    }
}

[Collection("AddressValidation")]
public sealed class AddressValidationServiceTests : IDisposable
{
    private const string BaseAddress = "https://api.test.com/";
    private readonly AddressValidationFixture fixture;
    private readonly HttpClient httpClient;
    private readonly AddressValidationService service;
    private bool disposed;

    public AddressValidationServiceTests(AddressValidationFixture fixture)
    {
        this.fixture = fixture;
        this.httpClient = new HttpClient(this.fixture.HttpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress),
        };
        this.service = new AddressValidationService(this.httpClient);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                this.httpClient.Dispose();
            }

            this.disposed = true;
        }
    }

    [Fact]
    public async Task GetAddressesAsyncSuccessfulRequestReturnsResponse()
    {
        // Arrange
        var request = new AddressValidationRequest(
            CompanyName: "Test Company",
            Line1: "123 Main St",
            Line2: null,
            City: "Test City",
            StateCode: "TS",
            Urbanization: null,
            ZipCodeLeading5: "12345",
            ZipCodeTrailing4: null);

        var expectedResponse = "{\"status\": \"success\"}";
        using var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(expectedResponse),
        };
        this.fixture.HttpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        // Act
        var result = await this.service.GetAddressesAsync(request).ConfigureAwait(true);

        // Assert
        Assert.Equal(expectedResponse, result);
        this.VerifyHttpCall(Times.Once());
    }

    [Fact]
    public async Task GetAddressesAsyncServerErrorRetriesAndSucceeds()
    {
        // Arrange
        var request = new AddressValidationRequest(
            CompanyName: "Test Company",
            Line1: "123 Main St",
            Line2: null,
            City: null,
            StateCode: null,
            Urbanization: null,
            ZipCodeLeading5: null,
            ZipCodeTrailing4: null);

        var expectedResponse = "{\"status\": \"success\"}";
        using var errorResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent("error"),
        };
        using var successResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(expectedResponse),
        };

        var setup = this.fixture.HttpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        setup.ReturnsAsync(errorResponse)
             .ReturnsAsync(successResponse);

        // Act
        var result = await this.service.GetAddressesAsync(request).ConfigureAwait(true);

        // Assert
        Assert.Equal(expectedResponse, result);
        this.VerifyHttpCall(Times.Exactly(2));
    }

    [Fact]
    public async Task GetAddressesAsyncClientErrorThrowsImmediately()
    {
        // Arrange
        var request = new AddressValidationRequest(
            CompanyName: "Test Company",
            Line1: "123 Main St",
            Line2: null,
            City: null,
            StateCode: null,
            Urbanization: null,
            ZipCodeLeading5: null,
            ZipCodeTrailing4: null);

        using var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent("error"),
        };
        this.fixture.HttpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => 
            this.service.GetAddressesAsync(request)).ConfigureAwait(true);
        Assert.Equal(400, (int)exception.StatusCode!);
        this.VerifyHttpCall(Times.Once());
    }

    [Fact]
    public async Task GetAddressesAsyncTimeoutRetriesAndSucceeds()
    {
        // Arrange
        var request = new AddressValidationRequest(
            CompanyName: "Test Company",
            Line1: "123 Main St",
            Line2: null,
            City: null,
            StateCode: null,
            Urbanization: null,
            ZipCodeLeading5: null,
            ZipCodeTrailing4: null);

        var expectedResponse = "{\"status\": \"success\"}";
        using var timeoutResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.RequestTimeout,
            Content = new StringContent("timeout"),
        };
        using var successResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(expectedResponse),
        };

        var setup = this.fixture.HttpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        setup.ReturnsAsync(timeoutResponse)
             .ReturnsAsync(successResponse);

        // Act
        var result = await this.service.GetAddressesAsync(request).ConfigureAwait(true);

        // Assert
        Assert.Equal(expectedResponse, result);
        this.VerifyHttpCall(Times.Exactly(2));
    }

    [Fact]
    public async Task GetAddressesAsyncMaxRetriesExceededThrowsException()
    {
        // Arrange
        var request = new AddressValidationRequest(
            CompanyName: "Test Company",
            Line1: "123 Main St",
            Line2: null,
            City: null,
            StateCode: null,
            Urbanization: null,
            ZipCodeLeading5: null,
            ZipCodeTrailing4: null);

        using var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent("error"),
        };
        this.fixture.HttpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => 
            this.service.GetAddressesAsync(request)).ConfigureAwait(true);
        Assert.Equal(500, (int)exception.StatusCode!);
        this.VerifyHttpCall(Times.Exactly(4));
    }

    private void VerifyHttpCall(Times times)
    {
        this.fixture.HttpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                times,
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }
} 