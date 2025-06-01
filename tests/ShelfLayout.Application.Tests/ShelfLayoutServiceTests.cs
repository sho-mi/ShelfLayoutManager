using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Interfaces;
using ShelfLayout.Application.Services;

namespace ShelfLayout.Application.Tests
{
    public class ShelfLayoutServiceTests
    {
        private readonly Mock<ISkuRepository> _skuRepositoryMock;
        private readonly Mock<IShelfRepository> _shelfRepositoryMock;
        private readonly ShelfLayoutService _service;

        public ShelfLayoutServiceTests()
        {
            _skuRepositoryMock = new Mock<ISkuRepository>();
            _shelfRepositoryMock = new Mock<IShelfRepository>();
            _service = new ShelfLayoutService(_skuRepositoryMock.Object, _shelfRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllCabinetsAsync_ShouldReturnCabinets()
        {
            // Arrange
            var expectedCabinets = new List<Cabinet>
            {
                new Cabinet { Number = 1 },
                new Cabinet { Number = 2 }
            };
            _shelfRepositoryMock.Setup(r => r.GetAllCabinetsAsync())
                .ReturnsAsync(expectedCabinets);

            // Act
            var result = await _service.GetAllCabinetsAsync();

            // Assert
            Assert.Equal(expectedCabinets, result);
            _shelfRepositoryMock.Verify(r => r.GetAllCabinetsAsync(), Times.Once);
        }

        [Fact]
        public async Task AddSkuToLaneAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var janCode = "4901234567890";
            var sku = new Sku 
            { 
                JanCode = janCode,
                Name = "Test SKU",
                Size = 355,
                Width = 1,
                Depth = 1,
                Height = 1,
                ShapeType = "Can",
                ImageUrl = "test.png",
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            var lane = new Lane 
            { 
                Number = 1, 
                Quantity = 5,
                JanCode = null,
                PositionX = 0
            };

            _skuRepositoryMock.Setup(r => r.GetByJanCodeAsync(janCode))
                .ReturnsAsync(sku);
            _shelfRepositoryMock.Setup(r => r.GetLaneAsync(1, 1, 1))
                .ReturnsAsync(lane);

            // Act
            await _service.AddSkuToLaneAsync(janCode, 1, 1, 1);

            // Assert
            _shelfRepositoryMock.Verify(r => r.UpdateLaneAsync(1, 1, It.IsAny<Lane>()), Times.Once);
        }

        [Fact]
        public async Task AddSkuToLaneAsync_WithInvalidSku_ShouldThrowException()
        {
            // Arrange
            var janCode = "invalid";
            _skuRepositoryMock.Setup(r => r.GetByJanCodeAsync(janCode))
                .ReturnsAsync((Sku)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddSkuToLaneAsync(janCode, 1, 1, 1));
        }

        [Fact]
        public async Task AddSkuToLaneAsync_WithInvalidLane_ShouldThrowException()
        {
            // Arrange
            var janCode = "4901234567890";
            var sku = new Sku 
            { 
                JanCode = janCode,
                Name = "Test SKU",
                Size = 355,
                Width = 1,
                Depth = 1,
                Height = 1,
                ShapeType = "Can",
                ImageUrl = "test.png",
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            _skuRepositoryMock.Setup(r => r.GetByJanCodeAsync(janCode))
                .ReturnsAsync(sku);
            _shelfRepositoryMock.Setup(r => r.GetLaneAsync(1, 1, 1))
                .ReturnsAsync((Lane)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddSkuToLaneAsync(janCode, 1, 1, 1));
        }

        [Fact]
        public async Task AddSkuToLaneAsync_WithOccupiedLane_ShouldThrowException()
        {
            // Arrange
            var janCode = "4901234567890";
            var sku = new Sku 
            { 
                JanCode = janCode,
                Name = "Test SKU",
                Size = 355,
                Width = 1,
                Depth = 1,
                Height = 1,
                ShapeType = "Can",
                ImageUrl = "test.png",
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            var lane = new Lane 
            { 
                Number = 1, 
                Quantity = 5,
                JanCode = "existing",
                PositionX = 0
            };

            _skuRepositoryMock.Setup(r => r.GetByJanCodeAsync(janCode))
                .ReturnsAsync(sku);
            _shelfRepositoryMock.Setup(r => r.GetLaneAsync(1, 1, 1))
                .ReturnsAsync(lane);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddSkuToLaneAsync(janCode, 1, 1, 1));
        }

        [Fact]
        public async Task MoveSkuAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var janCode = "4901234567890";
            var sku = new Sku 
            { 
                JanCode = janCode,
                Name = "Test SKU",
                Size = 355,
                Width = 1,
                Depth = 1,
                Height = 1,
                ShapeType = "Can",
                ImageUrl = "test.png",
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            var fromLane = new Lane 
            { 
                Number = 1, 
                Quantity = 5,
                JanCode = janCode,
                PositionX = 0
            };
            var toLane = new Lane 
            { 
                Number = 2, 
                Quantity = 5,
                JanCode = null,
                PositionX = 0
            };

            _skuRepositoryMock.Setup(r => r.GetByJanCodeAsync(janCode))
                .ReturnsAsync(sku);
            _shelfRepositoryMock.Setup(r => r.GetLaneAsync(1, 1, 1))
                .ReturnsAsync(fromLane);
            _shelfRepositoryMock.Setup(r => r.GetLaneAsync(1, 1, 2))
                .ReturnsAsync(toLane);

            // Act
            await _service.MoveSkuAsync(janCode, 1, 1, 1, 1, 1, 2);

            // Assert
            _shelfRepositoryMock.Verify(r => r.UpdateLaneAsync(1, 1, It.IsAny<Lane>()), Times.Exactly(2));
        }

        [Fact]
        public async Task RemoveSkuAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var janCode = "4901234567890";
            var lane = new Lane 
            { 
                Number = 1, 
                Quantity = 5,
                JanCode = janCode,
                PositionX = 0
            };

            _shelfRepositoryMock.Setup(r => r.GetLaneAsync(1, 1, 1))
                .ReturnsAsync(lane);

            // Act
            await _service.RemoveSkuAsync(janCode, 1, 1, 1);

            // Assert
            _shelfRepositoryMock.Verify(r => r.UpdateLaneAsync(1, 1, It.IsAny<Lane>()), Times.Once);
        }

        [Fact]
        public async Task RemoveSkuAsync_WithInvalidLane_ShouldThrowException()
        {
            // Arrange
            var janCode = "4901234567890";
            _shelfRepositoryMock.Setup(r => r.GetLaneAsync(1, 1, 1))
                .ReturnsAsync((Lane)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.RemoveSkuAsync(janCode, 1, 1, 1));
        }

        [Fact]
        public async Task RemoveSkuAsync_WithWrongSku_ShouldThrowException()
        {
            // Arrange
            var janCode = "4901234567890";
            var lane = new Lane 
            { 
                Number = 1, 
                Quantity = 5,
                JanCode = "different",
                PositionX = 0
            };

            _shelfRepositoryMock.Setup(r => r.GetLaneAsync(1, 1, 1))
                .ReturnsAsync(lane);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RemoveSkuAsync(janCode, 1, 1, 1));
        }
    }
} 