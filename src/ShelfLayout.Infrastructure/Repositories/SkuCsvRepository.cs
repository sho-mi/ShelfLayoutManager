using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace ShelfLayout.Infrastructure.Repositories
{
    public class SkuCsvRepository : ISkuRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SkuCsvRepository> _logger;
        private const string CACHE_KEY = "sku_data";
        private const int CACHE_DURATION_SECONDS = 5;
        private List<Sku> _skus;
        private bool _isInitialized;

        public SkuCsvRepository(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<SkuCsvRepository> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ShelfLayoutAPI");
            _cache = cache;
            _logger = logger;
            _skus = new List<Sku>();
            _httpClient.BaseAddress = new Uri("https://localhost:5237/");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/csv");
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
                // Try to get from cache first if available
                if (_cache.TryGetValue(CACHE_KEY, out List<Sku>? cachedSkus) && cachedSkus != null)
                {
                    _logger.LogInformation("Returning SKU data from cache");
                    _skus = cachedSkus;
                    return;
                }

                _logger.LogInformation("Attempting to load SKU data from api/shelflayout/sku-data");
                var response = await _httpClient.GetAsync("api/shelflayout/sku-data");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to load SKU data. Status code: {StatusCode}", response.StatusCode);
                    _skus = new List<Sku>();
                    return;
                }

                var csvContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully read {Length} characters from file", csvContent.Length);

                using var reader = new StringReader(csvContent);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null
                });

                _skus = csv.GetRecords<Sku>().ToList();
                _logger.LogInformation("Successfully loaded {Count} SKUs", _skus.Count);
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(CACHE_DURATION_SECONDS));
                _cache.Set(CACHE_KEY, _skus, cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SKU data: {Message}", ex.Message);
                _skus = new List<Sku>();
            }
        }

        private async Task SaveDataAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to save SKU data");
                var response = await _httpClient.PostAsJsonAsync("api/shelflayout/save-skus", _skus);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = $"Failed to save SKU data. Status code: {response.StatusCode}";
                    _logger.LogError(error);
                    throw new Exception(error);
                }
                
                _logger.LogInformation("Successfully saved SKU data");

                // Update cache after successful save
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(CACHE_DURATION_SECONDS));
                _cache.Set(CACHE_KEY, _skus, cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving SKU data");
                throw;
            }
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
            var sku = _skus.FirstOrDefault(s => s.JanCode == janCode);
            if (sku != null)
            {
                _skus.Remove(sku);
                await SaveDataAsync();
            }
        }
    }
} 