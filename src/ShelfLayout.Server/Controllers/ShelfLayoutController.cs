using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Interfaces;
using ShelfLayout.Core.Models;
using ShelfLayout.Server.Hubs;
using System.Text.Json;

namespace ShelfLayout.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShelfLayoutController : ControllerBase
{
    private readonly ILogger<ShelfLayoutController> _logger;
    private readonly IShelfRepository _shelfRepository;
    private readonly ISkuRepository _skuRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IHubContext<ShelfLayoutHub, IShelfLayoutHub> _hubContext;

    public ShelfLayoutController(
        ILogger<ShelfLayoutController> logger, 
        IShelfRepository shelfRepository,
        ISkuRepository skuRepository,
        IWebHostEnvironment environment,
        IHubContext<ShelfLayoutHub, IShelfLayoutHub> hubContext)
    {
        _logger = logger;
        _shelfRepository = shelfRepository;
        _skuRepository = skuRepository;
        _environment = environment;
        _hubContext = hubContext;
    }

    [HttpGet("shelf-data")]
    public async Task<ActionResult<ShelfData>> GetShelfData()
    {
        try
        {
            _logger.LogInformation("Received request for shelf data");
            
            var filePath = Path.Combine(_environment.ContentRootPath, "data", "shelf.json");
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("shelf.json not found at {FilePath}", filePath);
                return NotFound();
            }

            var jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                _logger.LogWarning("shelf.json is empty");
                return NotFound();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<ShelfData>(jsonContent, options);
            if (data == null)
            {
                _logger.LogWarning("Failed to deserialize shelf data");
                return NotFound();
            }

            _logger.LogInformation("Successfully loaded shelf data with {Count} cabinets", data.Cabinets?.Count ?? 0);
            
            // Add CORS headers explicitly
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            
            // Set content type explicitly
            Response.ContentType = "application/json";
            
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading shelf data: {Message}", ex.Message);
            return StatusCode(500, $"Error loading shelf data: {ex.Message}");
        }
    }

    [HttpGet("sku-data")]
    public async Task<IActionResult> GetSkuDataFile()
    {
        try
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "data", "sku.csv");
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("sku.csv not found at {FilePath}", filePath);
                return NotFound();
            }

            var csvContent = await System.IO.File.ReadAllTextAsync(filePath);
            Response.Headers.Add("Content-Disposition", "inline");
            return Content(csvContent, "text/csv; charset=utf-8");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading sku.csv");
            return StatusCode(500, "Failed to read SKU data");
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveShelfData([FromBody] ShelfData data)
    {
        try
        {
            // Log validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                _logger.LogError("Validation errors: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            // Log the data being saved
            _logger.LogInformation("Saving shelf data with {Count} cabinets", data?.Cabinets?.Count ?? 0);

            if (data?.Cabinets != null)
            {
                foreach (var cabinet in data.Cabinets)
                {
                    _logger.LogInformation("Cabinet {Number} has {RowCount} rows", 
                        cabinet.Number, 
                        cabinet.Rows?.Count ?? 0);

                    if (cabinet.Rows != null)
                    {
                        foreach (var row in cabinet.Rows)
                        {
                            _logger.LogInformation("Row {Number} has {LaneCount} lanes", 
                                row.Number, 
                                row.Lanes?.Count ?? 0);

                            if (row.Lanes != null)
                            {
                                foreach (var lane in row.Lanes)
                                {
                                    _logger.LogInformation("Lane {Number} has JanCode {JanCode} and Quantity {Quantity}", 
                                        lane.Number, 
                                        lane.JanCode, 
                                        lane.Quantity);
                                }
                            }
                        }
                    }
                }
            }

            // Log the raw request data
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var rawData = JsonSerializer.Serialize(data, jsonOptions);
            _logger.LogInformation("Raw request data: {Data}", rawData);

            // Ensure the data directory exists
            var dataDir = Path.Combine(_environment.ContentRootPath, "data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
                _logger.LogInformation("Created data directory at {Path}", dataDir);
            }

            // Save the data to file
            var filePath = Path.Combine(dataDir, "shelf.json");
            await System.IO.File.WriteAllTextAsync(filePath, rawData);
            _logger.LogInformation("Successfully saved shelf data to {FilePath}", filePath);

            // Notify all clients about the update
            if (_hubContext != null)
            {
                var clients = _hubContext.Clients.All as IClientProxy;
                if (clients != null)
                {
                    await clients.SendAsync("OnShelfLayoutUpdated");
                    _logger.LogInformation("Notified clients about shelf layout update");
                }
            }

            return Ok();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization error: {Message}", ex.Message);
            return BadRequest($"Invalid JSON data: {ex.Message}");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "File I/O error: {Message}", ex.Message);
            return StatusCode(500, $"Error saving file: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving shelf data: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while saving the data");
        }
    }

    [HttpPost("save-skus")]
    public async Task<IActionResult> SaveSkuData([FromBody] List<Sku> skus)
    {
        try
        {
            _logger.LogInformation("Attempting to save SKU data");
            
            // Ensure the data directory exists
            var dataDir = Path.Combine(_environment.ContentRootPath, "data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            var filePath = Path.Combine(dataDir, "sku.csv");
            var csvContent = "JanCode,Name,Width,Depth,Height,ImageUrl,Size,TimeStamp,ShapeType\n";
            
            foreach (var sku in skus)
            {
                csvContent += $"{sku.JanCode},{sku.Name},{sku.Width},{sku.Depth},{sku.Height},{sku.ImageUrl},{sku.Size},{sku.TimeStamp},{sku.ShapeType}\n";
            }
            
            await System.IO.File.WriteAllTextAsync(filePath, csvContent);
            
            _logger.LogInformation("Successfully saved SKU data to {FilePath}", filePath);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving SKU data");
            return StatusCode(500, "Failed to save SKU data");
        }
    }

    [HttpGet("data")]
    public async Task<IActionResult> GetShelfDataRaw()
    {
        try
        {
            var cabinets = await _shelfRepository.GetAllCabinetsAsync();
            return Ok(cabinets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shelf data");
            return StatusCode(500, "Failed to retrieve shelf data");
        }
    }
} 