using System.Text.Json.Serialization;

namespace ShelfLayout.Core.Entities
{
    public class Cabinet
    {
        public int Number { get; set; }
        public List<Row> Rows { get; set; } = new List<Row>();
        public Position Position { get; set; } = new Position();
        public Size Size { get; set; } = new Size();
    }

    public class Row
    {
        public int Number { get; set; }
        public List<Lane> Lanes { get; set; } = new List<Lane>();
        public decimal PositionZ { get; set; }
        public Size? Size { get; set; }
    }

    public class Position
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Z { get; set; }
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