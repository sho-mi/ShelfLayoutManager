using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Interfaces;
using System.Net.Http;

namespace ShelfLayout.Infrastructure.Repositories
{
    public class SkuCsvRepository : ISkuRepository
    {
        private readonly string _filePath;
        private readonly HttpClient _httpClient;
        private List<Sku> _skus;
        private bool _isInitialized;

        public SkuCsvRepository(HttpClient httpClient)
        {
            _filePath = "data/sku.csv";
            _httpClient = httpClient;
            _skus = new List<Sku>();
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
                var response = await _httpClient.GetAsync(_filePath);
                response.EnsureSuccessStatusCode();
                var csvContent = await response.Content.ReadAsStringAsync();
                
                var lines = csvContent.Split('\n').Skip(1); // Skip header
                _skus = new List<Sku>();
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var values = line.Split(',');
                    if (values.Length < 9) continue;

                    var sku = new Sku
                    {
                        JanCode = values[0].Trim(),
                        Name = values[1].Trim(),
                        Width = decimal.Parse(values[2].Trim()),
                        Depth = decimal.Parse(values[3].Trim()),
                        Height = decimal.Parse(values[4].Trim()),
                        ImageUrl = values[5].Trim(),
                        Size = int.Parse(values[6].Trim()),
                        TimeStamp = long.Parse(values[7].Trim()),
                        ShapeType = values[8].Trim()
                    };
                    
                    _skus.Add(sku);
                }
                
                if (_skus.Count > 0)
                {
                    var firstSku = _skus[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading SKUs: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                _skus = new List<Sku>();
            }
        }

        private async Task SaveDataAsync()
        {
            // Note: In Blazor WebAssembly, we can't write to the file system directly
            // This would need to be handled by a server-side API
            Console.WriteLine("Warning: SaveDataAsync is not implemented in WebAssembly");
        }

        public async Task<IEnumerable<Sku>> GetAllAsync()
        {
            await EnsureInitializedAsync();
            return _skus.AsEnumerable();
        }

        public async Task<Sku?> GetByJanCodeAsync(string janCode)
        {
            await EnsureInitializedAsync();
            return _skus.FirstOrDefault(s => s.JanCode == janCode);
        }

        public async Task AddAsync(Sku sku)
        {
            await EnsureInitializedAsync();
            if (_skus.Any(s => s.JanCode == sku.JanCode))
            {
                throw new InvalidOperationException($"SKU with JAN code {sku.JanCode} already exists.");
            }

            _skus.Add(sku);
            await SaveDataAsync();
        }

        public async Task UpdateAsync(Sku sku)
        {
            await EnsureInitializedAsync();
            var index = _skus.FindIndex(s => s.JanCode == sku.JanCode);
            if (index == -1)
            {
                throw new InvalidOperationException($"SKU with JAN code {sku.JanCode} not found.");
            }

            _skus[index] = sku;
            await SaveDataAsync();
        }

        public async Task DeleteAsync(string janCode)
        {
            await EnsureInitializedAsync();
            var index = _skus.FindIndex(s => s.JanCode == janCode);
            if (index == -1)
            {
                throw new InvalidOperationException($"SKU with JAN code {janCode} not found.");
            }

            _skus.RemoveAt(index);
            await SaveDataAsync();
        }
    }

    public class SkuCsvRecord
    {
        public string JanCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string X { get; set; } = string.Empty;
        public string Y { get; set; } = string.Empty;
        public string Z { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Size { get; set; }
        public string TimeStamp { get; set; } = string.Empty;
        public string Shape { get; set; } = string.Empty;
    }
} 