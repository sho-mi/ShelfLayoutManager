using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Interfaces;
using System.Net.Http;

namespace ShelfLayout.Infrastructure.Repositories
{
    public class JsonShelfRepository : IShelfRepository
    {
        private readonly string _fileName;
        private readonly HttpClient _httpClient;
        private List<Cabinet> _cabinets;
        private bool _isInitialized;

        public JsonShelfRepository(HttpClient httpClient)
        {
            _fileName = "data/shelf.json";
            _httpClient = httpClient;
            _cabinets = new List<Cabinet>();
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_isInitialized)
            {
                await LoadDataAsync();
                _isInitialized = true;
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_fileName);
                response.EnsureSuccessStatusCode();
                var jsonContent = await response.Content.ReadAsStringAsync();
                
                var data = JsonSerializer.Deserialize<ShelfData>(jsonContent);
                if (data?.Cabinets != null)
                {
                    _cabinets = data.Cabinets;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shelf data: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                _cabinets = new List<Cabinet>();
            }
        }

        private async Task SaveDataAsync()
        {
            var data = new ShelfData { Cabinets = _cabinets };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            // Note: In Blazor WebAssembly, we can't write to the file system directly
            // This would need to be handled by a server-side API
            Console.WriteLine("Warning: SaveDataAsync is not implemented in WebAssembly");
        }

        public async Task<IEnumerable<Cabinet>> GetAllCabinetsAsync()
        {
            await EnsureInitializedAsync();
            return _cabinets.AsEnumerable();
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
            await SaveDataAsync();
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
            await SaveDataAsync();
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
            await SaveDataAsync();
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
            await SaveDataAsync();
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
            await SaveDataAsync();
        }
    }

    public class ShelfData
    {
        public List<Cabinet> Cabinets { get; set; } = new List<Cabinet>();
    }
} 