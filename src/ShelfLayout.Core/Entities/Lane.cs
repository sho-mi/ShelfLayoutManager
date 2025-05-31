using System.Text.Json.Serialization;

namespace ShelfLayout.Core.Entities;

public class Lane
{
    public int Number { get; set; }
    public string? JanCode { get; set; }
    public int Quantity { get; set; }
    public decimal PositionX { get; set; }
    public int CabinetNumber { get; set; }
    public int RowNumber { get; set; }
} 