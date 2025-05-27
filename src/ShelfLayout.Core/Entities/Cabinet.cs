using System.Text.Json.Serialization;

namespace ShelfLayout.Core.Entities
{
    public class Cabinet
    {
        public required int Number { get; set; }
        public required List<Row> Rows { get; set; }
        public required Position Position { get; set; }
        public required Size Size { get; set; }
    }

    public class Row
    {
        public required int Number { get; set; }
        public required List<Lane> Lanes { get; set; }
        public required decimal PositionZ { get; set; }
        public Size? Size { get; set; }
    }

    public class Lane
    {
        public required int Number { get; set; }
        public string? JanCode { get; set; }
        public required int Quantity { get; set; }
        public required decimal PositionX { get; set; }
    }

    public class Position
    {
        public required decimal X { get; set; }
        public required decimal Y { get; set; }
        public required decimal Z { get; set; }
    }

    public class Size
    {
        [JsonPropertyName("Width")]
        public decimal Width { get; set; }

        [JsonPropertyName("Depth")]
        public decimal Depth { get; set; }

        [JsonPropertyName("Height")]
        public decimal Height { get; set; }
    }
} 