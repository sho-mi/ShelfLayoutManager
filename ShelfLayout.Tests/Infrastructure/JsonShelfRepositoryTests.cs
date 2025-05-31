using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Models;
using ShelfLayout.Infrastructure.Repositories;
using Xunit;

namespace ShelfLayout.Tests.Infrastructure
{
    public class JsonShelfRepositoryTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<JsonShelfRepository>> _loggerMock;
        private readonly IMemoryCache _cache;
        private readonly JsonShelfRepository _repository;
        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public JsonShelfRepositoryTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<JsonShelfRepository>>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:5237/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            _repository = new JsonShelfRepository(
                _httpClientFactoryMock.Object,
                _cache,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetAllCabinetsAsync_ShouldClearCacheAndLoadFreshData()
        {
            // Arrange
            var expectedCabinets = new List<Cabinet>
            {
                new Cabinet { Number = 1, Rows = new List<Row>() },
                new Cabinet { Number = 2, Rows = new List<Row>() }
            };

            var shelfData = new ShelfData { Cabinets = expectedCabinets };
            var jsonResponse = JsonSerializer.Serialize(shelfData);

            _httpMessageHandlerMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await _repository.GetAllCabinetsAsync();

            // Assert
            Assert.Equal(expectedCabinets.Count, result.Count());
            Assert.Equal(expectedCabinets[0].Number, result.First().Number);
            Assert.Equal(expectedCabinets[1].Number, result.Last().Number);
        }

        [Fact]
        public async Task SaveDataAsync_ShouldUpdateCacheAndNotifyClients()
        {
            // Arrange
            var cabinets = new List<Cabinet>
            {
                new Cabinet { Number = 1, Rows = new List<Row>() }
            };

            var shelfData = new ShelfData { Cabinets = cabinets };
            var jsonResponse = JsonSerializer.Serialize(shelfData);

            _httpMessageHandlerMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act
            await _repository.SaveDataAsync(shelfData);

            // Assert
            var cachedData = _cache.Get<ShelfData>("shelf_data");
            Assert.NotNull(cachedData);
            Assert.Equal(cabinets.Count, cachedData.Cabinets.Count);
            Assert.Equal(cabinets[0].Number, cachedData.Cabinets[0].Number);
        }

        [Fact]
        public async Task GetAllCabinetsAsync_ShouldHandleHttpError()
        {
            // Arrange
            _httpMessageHandlerMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server Error")
                });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _repository.GetAllCabinetsAsync());
        }

        [Fact]
        public async Task SaveDataAsync_ShouldHandleHttpError()
        {
            // Arrange
            var shelfData = new ShelfData { Cabinets = new List<Cabinet>() };

            _httpMessageHandlerMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server Error")
                });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _repository.SaveDataAsync(shelfData));
        }
    }
} 