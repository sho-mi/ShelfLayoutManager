using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Interfaces;
using ShelfLayout.Core.Models;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Http;

namespace ShelfLayout.Infrastructure.Repositories
{
    public class JsonShelfRepository : IShelfRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<JsonShelfRepository> _logger;
        private const string CACHE_KEY = "shelf_data";
        private const int CACHE_DURATION_SECONDS = 5;
        private List<Cabinet> _cabinets;
        private bool _isInitialized;

        public JsonShelfRepository(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<JsonShelfRepository> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ShelfLayoutAPI");
            _cache = cache;
            _logger = logger;
            _cabinets = new List<Cabinet>();
            _httpClient.BaseAddress = new Uri("https://localhost:5237/");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_isInitialized)
            {
                await LoadDataAsync();
                _isInitialized = true;
            }
        }

        public async Task<ShelfData> LoadDataAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to load shelf data from api/shelflayout/shelf-data");
                var response = await _httpClient.GetAsync("api/shelflayout/shelf-data");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to load shelf data. Status code: {StatusCode}. Error: {Error}", 
                        response.StatusCode, errorContent);
                    _cabinets = new List<Cabinet>();
                    return new ShelfData { Cabinets = _cabinets };
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully read {Length} characters from file", jsonContent.Length);

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    _logger.LogWarning("Received empty JSON content");
                    return new ShelfData { Cabinets = new List<Cabinet>() };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    PropertyNameCaseInsensitive = true
                };

                try
                {
                    var shelfData = JsonSerializer.Deserialize<ShelfData>(jsonContent, options);
                    if (shelfData == null)
                    {
                        _logger.LogWarning("Deserialized shelf data is null");
                        return new ShelfData { Cabinets = new List<Cabinet>() };
                    }

                    _cabinets = shelfData.Cabinets;
                    return shelfData;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing shelf data: {Message}. Content: {Content}", 
                        ex.Message, jsonContent);
                    return new ShelfData { Cabinets = new List<Cabinet>() };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shelf data: {Message}", ex.Message);
                return new ShelfData { Cabinets = new List<Cabinet>() };
            }
        }

        public async Task SaveDataAsync(ShelfData data)
        {
            try
            {
                _logger.LogInformation("Attempting to save shelf data with {Count} cabinets", data?.Cabinets?.Count ?? 0);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                // Log the request data
                var requestJson = JsonSerializer.Serialize(data, options);
                _logger.LogInformation("Request data: {Json}", requestJson);

                // Log the first cabinet's data for debugging
                if (data?.Cabinets?.Any() == true)
                {
                    var firstCabinet = data.Cabinets.First();
                    _logger.LogInformation("First cabinet data - Number: {Number}, Rows: {RowCount}", 
                        firstCabinet.Number, 
                        firstCabinet.Rows?.Count ?? 0);

                    // Log the first row's data if it exists
                    if (firstCabinet.Rows?.Any() == true)
                    {
                        var firstRow = firstCabinet.Rows.First();
                        _logger.LogInformation("First row data - Number: {Number}, Lanes: {LaneCount}", 
                            firstRow.Number,
                            firstRow.Lanes?.Count ?? 0);

                        // Log the first lane's data if it exists
                        if (firstRow.Lanes?.Any() == true)
                        {
                            var firstLane = firstRow.Lanes.First();
                            _logger.LogInformation("First lane data - Number: {Number}, JanCode: {JanCode}, PositionX: {PositionX}", 
                                firstLane.Number,
                                firstLane.JanCode,
                                firstLane.PositionX);
                        }
                    }
                }

                var response = await _httpClient.PostAsJsonAsync("api/shelflayout/save", data, options);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to save shelf data. Status code: {StatusCode}. Error: {Error}", 
                        response.StatusCode, errorContent);
                    throw new Exception($"Failed to save shelf data. Status code: {response.StatusCode}. Error: {errorContent}");
                }

                _cabinets = data?.Cabinets ?? new List<Cabinet>();
                _logger.LogInformation("Successfully saved shelf data");

                // Update cache after successful save
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(CACHE_DURATION_SECONDS));
                _cache.Set(CACHE_KEY, data, cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving shelf data: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Cabinet>> GetAllCabinetsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all cabinets");
                
                // Clear the cache to ensure fresh data
                if (_cache != null)
                {
                    _cache.Remove(CACHE_KEY);
                }
                
                // Reset initialization state
                _isInitialized = false;
                
                // Load fresh data
                var shelfData = await LoadDataAsync();
                _cabinets = shelfData.Cabinets;
                
                _logger.LogInformation("Successfully loaded {Count} cabinets", _cabinets.Count);
                return _cabinets.AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all cabinets: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Cabinet?> GetCabinetByNumberAsync(int number)
        {
            await EnsureInitializedAsync();
            return _cabinets.FirstOrDefault(c => c.Number == number);
        }

        public async Task AddCabinetAsync(Cabinet cabinet)
        {
            if (_cabinets.Any(c => c.Number == cabinet.Number))
            {
                throw new InvalidOperationException($"Cabinet with number {cabinet.Number} already exists.");
            }

            _cabinets.Add(cabinet);
            await SaveDataAsync(new ShelfData { Cabinets = _cabinets });
        }

        public async Task UpdateCabinetAsync(Cabinet cabinet)
        {
            await EnsureInitializedAsync();
            var index = _cabinets.FindIndex(c => c.Number == cabinet.Number);
            if (index == -1)
            {
                throw new InvalidOperationException($"Cabinet {cabinet.Number} not found.");
            }

            _cabinets[index] = cabinet;
            await SaveDataAsync(new ShelfData { Cabinets = _cabinets });
        }

        public async Task DeleteCabinetAsync(int number)
        {
            await EnsureInitializedAsync();
            var index = _cabinets.FindIndex(c => c.Number == number);
            if (index == -1)
            {
                throw new InvalidOperationException($"Cabinet {number} not found.");
            }

            _cabinets.RemoveAt(index);
            await SaveDataAsync(new ShelfData { Cabinets = _cabinets });
        }

        public async Task<Row?> GetRowAsync(int cabinetNumber, int rowNumber)
        {
            await EnsureInitializedAsync();
            var cabinet = _cabinets.FirstOrDefault(c => c.Number == cabinetNumber);
            return cabinet?.Rows.FirstOrDefault(r => r.Number == rowNumber);
        }

        public async Task UpdateRowAsync(int cabinetNumber, Row row)
        {
            await EnsureInitializedAsync();
            var cabinet = _cabinets.FirstOrDefault(c => c.Number == cabinetNumber);
            if (cabinet == null)
            {
                throw new InvalidOperationException($"Cabinet {cabinetNumber} not found.");
            }

            var index = cabinet.Rows.FindIndex(r => r.Number == row.Number);
            if (index == -1)
            {
                throw new InvalidOperationException($"Row {row.Number} not found in cabinet {cabinetNumber}.");
            }

            cabinet.Rows[index] = row;
            await SaveDataAsync(new ShelfData { Cabinets = _cabinets });
        }

        public async Task<Lane?> GetLaneAsync(int cabinetNumber, int rowNumber, int laneNumber)
        {
            await EnsureInitializedAsync();
            var cabinet = _cabinets.FirstOrDefault(c => c.Number == cabinetNumber);
            var row = cabinet?.Rows.FirstOrDefault(r => r.Number == rowNumber);
            return row?.Lanes.FirstOrDefault(l => l.Number == laneNumber);
        }

        public async Task UpdateLaneAsync(int cabinetNumber, int rowNumber, Lane lane)
        {
            await EnsureInitializedAsync();
            var cabinet = _cabinets.FirstOrDefault(c => c.Number == cabinetNumber);
            if (cabinet == null)
                throw new ArgumentException($"Cabinet {cabinetNumber} not found");

            var row = cabinet.Rows.FirstOrDefault(r => r.Number == rowNumber);
            if (row == null)
                throw new ArgumentException($"Row {rowNumber} not found in cabinet {cabinetNumber}");

            var laneIndex = row.Lanes.FindIndex(l => l.Number == lane.Number);
            if (laneIndex == -1)
                throw new ArgumentException($"Lane {lane.Number} not found in row {rowNumber} of cabinet {cabinetNumber}");

            row.Lanes[laneIndex] = lane;
            await SaveDataAsync(new ShelfData { Cabinets = _cabinets });
        }
    }
} 