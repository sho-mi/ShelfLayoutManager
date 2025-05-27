using System.Collections.Generic;
using System.Threading.Tasks;
using ShelfLayout.Core.Entities;

namespace ShelfLayout.Core.Interfaces
{
    public interface IShelfRepository
    {
        Task<IEnumerable<Cabinet>> GetAllCabinetsAsync();
        Task<Cabinet?> GetCabinetByNumberAsync(int number);
        Task AddCabinetAsync(Cabinet cabinet);
        Task UpdateCabinetAsync(Cabinet cabinet);
        Task DeleteCabinetAsync(int number);
        
        Task<Row?> GetRowAsync(int cabinetNumber, int rowNumber);
        Task UpdateRowAsync(int cabinetNumber, Row row);
        
        Task<Lane?> GetLaneAsync(int cabinetNumber, int rowNumber, int laneNumber);
        Task UpdateLaneAsync(int cabinetNumber, int rowNumber, Lane lane);
    }
} 