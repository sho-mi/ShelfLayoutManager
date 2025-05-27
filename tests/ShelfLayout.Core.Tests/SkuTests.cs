using System;
using Xunit;
using ShelfLayout.Core.Entities;

namespace ShelfLayout.Core.Tests
{
    public class SkuTests
    {
        [Fact]
        public void CreateSku_WithValidData_ShouldSucceed()
        {
            // Arrange
            var janCode = "4901234567890";
            var name = "Test SKU";
            var size = 355;
            var width = 6.5m;
            var depth = 6.5m;
            var height = 12.2m;
            var shapeType = "Can";
            var imageUrl = "test.png";
            var timeStamp = 1659397548L;

            // Act
            var sku = new Sku
            {
                JanCode = janCode,
                Name = name,
                Size = size,
                Width = width,
                Depth = depth,
                Height = height,
                ShapeType = shapeType,
                ImageUrl = imageUrl,
                TimeStamp = timeStamp
            };

            // Assert
            Assert.Equal(janCode, sku.JanCode);
            Assert.Equal(name, sku.Name);
            Assert.Equal(size, sku.Size);
            Assert.Equal(width, sku.Width);
            Assert.Equal(depth, sku.Depth);
            Assert.Equal(height, sku.Height);
            Assert.Equal(shapeType, sku.ShapeType);
            Assert.Equal(imageUrl, sku.ImageUrl);
            Assert.Equal(timeStamp, sku.TimeStamp);
            Assert.Equal("355ml", sku.DrinkSize);
            Assert.Equal(width, sku.ProductSize.Width);
            Assert.Equal(depth, sku.ProductSize.Depth);
            Assert.Equal(height, sku.ProductSize.Height);
        }
    }
} 