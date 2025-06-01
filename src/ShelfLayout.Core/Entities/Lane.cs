using System.Text.Json.Serialization;
using System.ComponentModel;

namespace ShelfLayout.Core.Entities;

public class Lane
{
    public int Number { get; set; }
    private string? _janCode;
    public string? JanCode 
    { 
        get => _janCode;
        set
        {
            if (value != null && _janCode != null)
            {
                throw new InvalidOperationException("Cannot add SKU to a lane that is already occupied");
            }
            _janCode = value;
        }
    }
    public int Quantity { get; set; }
    public decimal PositionX { get; set; }
    public int CabinetNumber { get; set; }
    public int RowNumber { get; set; }
} 