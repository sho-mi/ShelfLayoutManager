using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Interfaces;
using ShelfLayout.Web.Services;

namespace ShelfLayout.Web.Tests
{
    public class ShelfLayoutServiceTests
    {
        private readonly Mock<IShelfRepository> _shelfRepositoryMock;
        private readonly Mock<IShelfLayoutHubService> _hubServiceMock;
        private readonly ShelfLayoutService _service;

        public ShelfLayoutServiceTests()
        {
            _shelfRepositoryMock = new Mock<IShelfRepository>();
            _hubServiceMock = new Mock<IShelfLayoutHubService>();
            _hubServiceMock.Setup(x => x.DisposeAsync())
                .Returns(ValueTask.CompletedTask);
            _service = new ShelfLayoutService(_shelfRepositoryMock.Object, _hubServiceMock.Object);
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
            var cabinet = new Cabinet
            {
                Number = 1,
                Rows = new List<Row>
                {
                    new Row
                    {
                        Number = 1,
                        Lanes = new List<Lane>
                        {
                            new Lane
                            {
                                Number = 1,
                                Quantity = 5,
                                PositionX = 0
                            }
                        }
                    }
                }
            };

            _shelfRepositoryMock.Setup(r => r.GetCabinetByNumberAsync(1))
                .ReturnsAsync(cabinet);

            // Act
            await _service.AddSkuToLaneAsync(janCode, 1, 1, 1, 5);

            // Assert
            _shelfRepositoryMock.Verify(r => r.UpdateCabinetAsync(It.IsAny<Cabinet>()), Times.Once);
            _hubServiceMock.Verify(h => h.NotifyShelfLayoutUpdatedAsync(), Times.Once);
        }

        [Fact]
        public async Task AddSkuToLaneAsync_WithInvalidCabinet_ShouldReturn()
        {
            // Arrange
            _shelfRepositoryMock.Setup(r => r.GetCabinetByNumberAsync(1))
                .ReturnsAsync((Cabinet)null);

            // Act
            await _service.AddSkuToLaneAsync("4901234567890", 1, 1, 1);

            // Assert
            _shelfRepositoryMock.Verify(r => r.UpdateCabinetAsync(It.IsAny<Cabinet>()), Times.Never);
            _hubServiceMock.Verify(h => h.NotifyShelfLayoutUpdatedAsync(), Times.Never);
        }

        [Fact]
        public async Task RemoveSkuAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var cabinet = new Cabinet
            {
                Number = 1,
                Rows = new List<Row>
                {
                    new Row
                    {
                        Number = 1,
                        Lanes = new List<Lane>
                        {
                            new Lane
                            {
                                Number = 1,
                                Quantity = 5,
                                PositionX = 0,
                                JanCode = "4901234567890"
                            }
                        }
                    }
                }
            };

            _shelfRepositoryMock.Setup(r => r.GetCabinetByNumberAsync(1))
                .ReturnsAsync(cabinet);

            // Act
            await _service.RemoveSkuAsync("4901234567890", 1, 1, 1);

            // Assert
            _shelfRepositoryMock.Verify(r => r.UpdateCabinetAsync(It.IsAny<Cabinet>()), Times.Once);
            _hubServiceMock.Verify(h => h.NotifySkuRemovedAsync("4901234567890"), Times.Once);
            _hubServiceMock.Verify(h => h.NotifyShelfLayoutUpdatedAsync(), Times.Once);
        }

        [Fact]
        public async Task AddCabinetAsync_ShouldSucceed()
        {
            // Arrange
            var cabinet = new Cabinet { Number = 1 };

            // Act
            await _service.AddCabinetAsync(cabinet);

            // Assert
            _shelfRepositoryMock.Verify(r => r.AddCabinetAsync(cabinet), Times.Once);
            _hubServiceMock.Verify(h => h.NotifyCabinetManagementUpdatedAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCabinetAsync_ShouldSucceed()
        {
            // Arrange
            var cabinet = new Cabinet { Number = 1 };

            // Act
            await _service.UpdateCabinetAsync(cabinet);

            // Assert
            _shelfRepositoryMock.Verify(r => r.UpdateCabinetAsync(cabinet), Times.Once);
            _hubServiceMock.Verify(h => h.NotifyCabinetManagementUpdatedAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCabinetAsync_ShouldSucceed()
        {
            // Act
            await _service.DeleteCabinetAsync(1);

            // Assert
            _shelfRepositoryMock.Verify(r => r.DeleteCabinetAsync(1), Times.Once);
            _hubServiceMock.Verify(h => h.NotifyCabinetManagementUpdatedAsync(), Times.Once);
        }

        [Fact]
        public async Task AddRowToCabinetAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var cabinet = new Cabinet
            {
                Number = 1,
                Rows = new List<Row>()
            };
            var row = new Row { Number = 1 };

            _shelfRepositoryMock.Setup(r => r.GetCabinetByNumberAsync(1))
                .ReturnsAsync(cabinet);

            // Act
            await _service.AddRowToCabinetAsync(1, row);

            // Assert
            _shelfRepositoryMock.Verify(r => r.UpdateCabinetAsync(It.IsAny<Cabinet>()), Times.Once);
            _hubServiceMock.Verify(h => h.NotifyCabinetManagementUpdatedAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveRowFromCabinetAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var cabinet = new Cabinet
            {
                Number = 1,
                Rows = new List<Row>
                {
                    new Row { Number = 1 }
                }
            };

            _shelfRepositoryMock.Setup(r => r.GetCabinetByNumberAsync(1))
                .ReturnsAsync(cabinet);

            // Act
            await _service.RemoveRowFromCabinetAsync(1, 1);

            // Assert
            _shelfRepositoryMock.Verify(r => r.UpdateCabinetAsync(It.IsAny<Cabinet>()), Times.Once);
            _hubServiceMock.Verify(h => h.NotifyRowRemovedAsync(1, 1), Times.Once);
            _hubServiceMock.Verify(h => h.NotifyCabinetManagementUpdatedAsync(), Times.Once);
            _hubServiceMock.Verify(h => h.NotifyShelfLayoutUpdatedAsync(), Times.Once);
        }
    }
} 