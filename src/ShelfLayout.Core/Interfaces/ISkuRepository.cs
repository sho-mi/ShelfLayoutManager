using System.Collections.Generic;
using System.Threading.Tasks;
using ShelfLayout.Core.Entities;

namespace ShelfLayout.Core.Interfaces
{
    public interface ISkuRepository
    {
        Task<IEnumerable<Sku>> GetAllAsync();
        Task<Sku?> GetByJanCodeAsync(string janCode);
        Task AddAsync(Sku sku);
        Task UpdateAsync(Sku sku);
        Task DeleteAsync(string janCode);
    }
} 