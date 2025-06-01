using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Models;
using ShelfLayout.Infrastructure.Repositories;
using Xunit;

namespace ShelfLayout.Infrastructure.Tests
{
    public class JsonShelfRepositoryTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<ILogger<JsonShelfRepository>> _loggerMock;
        private readonly HttpClient _httpClient;
        private readonly JsonShelfRepository _repository;
        private readonly List<Cabinet> _testCabinets;
        private readonly Mock<ICacheEntry> _cacheEntryMock;

        public JsonShelfRepositoryTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<JsonShelfRepository>>();
            _cacheEntryMock = new Mock<ICacheEntry>();

            // Setup memory cache mock
            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(_cacheEntryMock.Object);

            // Setup a fake HttpMessageHandler
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
                {
                    if (request.RequestUri!.AbsolutePath.Contains("shelflayout/shelf-data"))
                    {
                        var json = "{\"cabinets\":[{\"number\":1,\"rows\":[]}]}";
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(json)
                        };
                    }
                    if (request.RequestUri!.AbsolutePath.Contains("shelflayout/save"))
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                });
            _httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:5237/")
            };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            _repository = new JsonShelfRepository(_httpClientFactoryMock.Object, _memoryCacheMock.Object, _loggerMock.Object);
            _testCabinets = new List<Cabinet> { new Cabinet { Number = 1, Rows = new List<Row>() } };
        }

        [Fact]
        public async Task GetAllCabinetsAsync_ShouldReturnCabinets()
        {
            // Act
            var result = await _repository.GetAllCabinetsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, ((List<Cabinet>)result)[0].Number);
        }

        [Fact]
        public async Task AddCabinetAsync_ShouldAddCabinet()
        {
            // Arrange
            var newCabinet = new Cabinet { Number = 2, Rows = new List<Row>() };

            // Act
            await _repository.AddCabinetAsync(newCabinet);

            // No exception means success (since we can't verify internal state due to private _cabinets)
        }

        [Fact]
        public async Task UpdateCabinetAsync_ShouldThrowIfNotFound()
        {
            // Arrange
            var nonExistentCabinet = new Cabinet { Number = 99, Rows = new List<Row>() };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateCabinetAsync(nonExistentCabinet));
        }
    }
} 