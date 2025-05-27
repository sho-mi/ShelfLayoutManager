using System;
using System.Linq;
using Xunit;
using ShelfLayout.Core.Entities;

namespace ShelfLayout.Core.Tests
{
    public class ShelfTests
    {
        [Fact]
        public void CreateCabinet_WithValidData_ShouldSucceed()
        {
            // Arrange
            var cabinetNumber = 1;
            var position = new Position { X = 0, Y = 0, Z = 0 };
            var size = new Size { Width = 1000, Depth = 500, Height = 2000 };

            // Act
            var cabinet = new Cabinet
            {
                Number = cabinetNumber,
                Position = position,
                Size = size,
                Rows = new List<Row>()
            };

            // Assert
            Assert.Equal(cabinetNumber, cabinet.Number);
            Assert.Equal(position, cabinet.Position);
            Assert.Equal(size, cabinet.Size);
            Assert.NotNull(cabinet.Rows);
            Assert.Empty(cabinet.Rows);
        }

        [Fact]
        public void AddRowToCabinet_ShouldSucceed()
        {
            // Arrange
            var cabinet = new Cabinet 
            { 
                Number = 1,
                Position = new Position { X = 0, Y = 0, Z = 0 },
                Size = new Size { Width = 1000, Depth = 500, Height = 2000 },
                Rows = new List<Row>()
            };
            var row = new Row
            {
                Number = 1,
                Lanes = new List<Lane>(),
                PositionZ = 0
            };

            // Act
            cabinet.Rows.Add(row);

            // Assert
            Assert.Single(cabinet.Rows);
            Assert.Equal(row, cabinet.Rows.First());
        }

        [Fact]
        public void AddLaneToRow_ShouldSucceed()
        {
            // Arrange
            var row = new Row 
            { 
                Number = 1,
                Lanes = new List<Lane>(),
                PositionZ = 0
            };
            var lane = new Lane
            {
                Number = 1,
                Quantity = 5,
                PositionX = 0,
                JanCode = "4901234567890"
            };

            // Act
            row.Lanes.Add(lane);

            // Assert
            Assert.Single(row.Lanes);
            Assert.Equal(lane, row.Lanes.First());
        }

        [Fact]
        public void AddSkuToLane_ShouldSucceed()
        {
            // Arrange
            var lane = new Lane
            {
                Number = 1,
                Quantity = 5,
                PositionX = 0,
                JanCode = "4901234567890"
            };
            var janCode = "4901234567890";

            // Act
            lane.JanCode = janCode;

            // Assert
            Assert.Equal(janCode, lane.JanCode);
        }

        [Fact]
        public void AddSkuToLane_WhenFull_ShouldThrowException()
        {
            // Arrange
            var lane = new Lane
            {
                Number = 1,
                Quantity = 1,
                PositionX = 0,
                JanCode = "4901234567890"
            };
            lane.JanCode = "4901234567890";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => lane.JanCode = "4901234567891");
        }
    }
} 