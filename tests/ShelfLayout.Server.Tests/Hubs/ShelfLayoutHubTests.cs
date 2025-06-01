using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Moq;
using ShelfLayout.Server.Hubs;
using Xunit;

namespace ShelfLayout.Server.Tests.Hubs
{
    public class ShelfLayoutHubTests
    {
        private readonly Mock<IHubCallerClients<IShelfLayoutHub>> _clientsMock;
        private readonly Mock<IShelfLayoutHub> _clientProxyMock;
        private readonly ShelfLayoutHub _hub;

        public ShelfLayoutHubTests()
        {
            _clientsMock = new Mock<IHubCallerClients<IShelfLayoutHub>>();
            _clientProxyMock = new Mock<IShelfLayoutHub>();
            _clientsMock.Setup(c => c.All).Returns(_clientProxyMock.Object);
            _hub = new ShelfLayoutHub
            {
                Clients = _clientsMock.Object
            };
        }

        [Fact]
        public async Task NotifyShelfLayoutUpdated_ShouldNotifyAllClients()
        {
            // Act
            await _hub.NotifyShelfLayoutUpdated();

            // Assert
            _clientProxyMock.Verify(c => c.OnShelfLayoutUpdated(), Times.Once);
        }

        [Fact]
        public async Task NotifySkuRemoved_ShouldNotifyAllClients()
        {
            // Arrange
            var janCode = "4901234567890";

            // Act
            await _hub.NotifySkuRemoved(janCode);

            // Assert
            _clientProxyMock.Verify(c => c.OnSkuRemoved(janCode), Times.Once);
        }

        [Fact]
        public async Task NotifySkuAdded_ShouldNotifyAllClients()
        {
            // Arrange
            var janCode = "4901234567890";

            // Act
            await _hub.NotifySkuAdded(janCode);

            // Assert
            _clientProxyMock.Verify(c => c.OnSkuAdded(janCode), Times.Once);
        }

        [Fact]
        public async Task NotifyRowRemoved_ShouldNotifyAllClients()
        {
            // Arrange
            var cabinetNumber = 1;
            var rowNumber = 2;

            // Act
            await _hub.NotifyRowRemoved(cabinetNumber, rowNumber);

            // Assert
            _clientProxyMock.Verify(c => c.OnRowRemoved(cabinetNumber, rowNumber), Times.Once);
        }
    }
} 