using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Interfaces;
using ShelfLayout.Core.Models;
using ShelfLayout.Server.Controllers;
using ShelfLayout.Server.Hubs;
using Xunit;

namespace ShelfLayout.Server.Tests.Controllers
{
    public class ShelfLayoutControllerTests : IDisposable
    {
        private readonly Mock<ILogger<ShelfLayoutController>> _loggerMock;
        private readonly Mock<IShelfRepository> _shelfRepositoryMock;
        private readonly Mock<ISkuRepository> _skuRepositoryMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly Mock<IHubContext<ShelfLayoutHub, IShelfLayoutHub>> _hubContextMock;
        private readonly ShelfLayoutController _controller;
        private readonly string _tempPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public ShelfLayoutControllerTests()
        {
            _loggerMock = new Mock<ILogger<ShelfLayoutController>>();
            _shelfRepositoryMock = new Mock<IShelfRepository>();
            _skuRepositoryMock = new Mock<ISkuRepository>();
            _environmentMock = new Mock<IWebHostEnvironment>();
            _hubContextMock = new Mock<IHubContext<ShelfLayoutHub, IShelfLayoutHub>>();

            _tempPath = Path.Combine(Path.GetTempPath(), "ShelfLayoutTests");
            Directory.CreateDirectory(_tempPath);
            _environmentMock.Setup(e => e.ContentRootPath).Returns(_tempPath);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };

            _controller = new ShelfLayoutController(
                _loggerMock.Object,
                _shelfRepositoryMock.Object,
                _skuRepositoryMock.Object,
                _environmentMock.Object,
                _hubContextMock.Object
            );
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempPath))
            {
                Directory.Delete(_tempPath, true);
            }
        }
        

        [Fact]
        public async Task SaveShelfData_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var shelfData = new ShelfData
            {
                Cabinets = new List<Cabinet>
                {
                    new Cabinet { Number = 1 }
                }
            };

            var clientsMock = new Mock<IShelfLayoutHub>();
            _hubContextMock.Setup(h => h.Clients.All).Returns(clientsMock.Object);

            // Act
            var result = await _controller.SaveShelfData(shelfData);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _hubContextMock.Verify(h => h.Clients.All, Times.Once);
        }

        [Fact]
        public async Task SaveSkuData_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var skus = new List<Sku>
            {
                new Sku
                {
                    JanCode = "4901234567890",
                    Name = "Test SKU",
                    Width = 10,
                    Depth = 10,
                    Height = 10,
                    Size = 500,
                    ImageUrl = "https://example.com/image.jpg",
                    TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    ShapeType = "Box"
                }
            };

            // Act
            var result = await _controller.SaveSkuData(skus);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
        }
    }
} 