using System;
using CsvHelper.Configuration.Attributes;

namespace ShelfLayout.Core.Entities
{
    public class Sku
    {
        [Name("JanCode")]
        public required string JanCode { get; set; }

        [Name("Name")]
        public required string Name { get; set; }

        [Name("Size")]
        public required int Size { get; set; }

        [Name("X")]
        public required decimal Width { get; set; }

        [Name("Y")]
        public required decimal Depth { get; set; }

        [Name("Z")]
        public required decimal Height { get; set; }

        [Name("ImageURL")]
        public required string ImageUrl { get; set; }

        [Name("TimeStamp")]
        public required long TimeStamp { get; set; }

        [Name("Shape")]
        public required string ShapeType { get; set; }

        public string DrinkSize => $"{Size}ml";

        public ProductSize ProductSize => new()
        {
            Width = Width,
            Depth = Depth,
            Height = Height
        };

        public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(TimeStamp).DateTime;
    }

    public class ProductSize
    {
        public required decimal Width { get; set; }  // X
        public required decimal Depth { get; set; }  // Y
        public required decimal Height { get; set; } // Z
    }
} 