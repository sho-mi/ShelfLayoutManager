using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ShelfLayout.Web.Services;

public class ShelfLayoutService
{
    private readonly IShelfRepository _shelfRepository;
    private readonly IShelfLayoutHubService _hubService;

    public ShelfLayoutService(
        IShelfRepository shelfRepository, 
        IShelfLayoutHubService hubService)
    {
        _shelfRepository = shelfRepository;
        _hubService = hubService;
    }

    public async Task<List<Cabinet>> GetAllCabinetsAsync()
    {
        var cabinets = await _shelfRepository.GetAllCabinetsAsync();
        return cabinets.ToList();
    }

    public async Task AddSkuToLaneAsync(string janCode, int cabinetNumber, int rowNumber, int laneNumber, int quantity = 1)
    {
        var cabinet = await _shelfRepository.GetCabinetByNumberAsync(cabinetNumber);
        if (cabinet == null) return;

        var row = cabinet.Rows.FirstOrDefault(r => r.Number == rowNumber);
        if (row == null) return;

        var lane = row.Lanes.FirstOrDefault(l => l.Number == laneNumber);
        if (lane == null) return;

        lane.JanCode = janCode;
        lane.Quantity = quantity;
        lane.CabinetNumber = cabinetNumber;
        lane.RowNumber = rowNumber;

        await _shelfRepository.UpdateCabinetAsync(cabinet);
        await _hubService.NotifyShelfLayoutUpdatedAsync();
    }

    public async Task RemoveSkuAsync(string janCode, int cabinetNumber, int rowNumber, int laneNumber)
    {
        var cabinet = await _shelfRepository.GetCabinetByNumberAsync(cabinetNumber);
        if (cabinet == null) return;

        var row = cabinet.Rows.FirstOrDefault(r => r.Number == rowNumber);
        if (row == null) return;

        var lane = row.Lanes.FirstOrDefault(l => l.Number == laneNumber);
        if (lane == null) return;

        lane.JanCode = null;
        lane.Quantity = 0;
        lane.CabinetNumber = cabinetNumber;
        lane.RowNumber = rowNumber;

        await _shelfRepository.UpdateCabinetAsync(cabinet);
        await _hubService.NotifySkuRemovedAsync(janCode);
        await _hubService.NotifyShelfLayoutUpdatedAsync();
    }

    public async Task AddCabinetAsync(Cabinet cabinet)
    {
        await _shelfRepository.AddCabinetAsync(cabinet);
        await _hubService.NotifyCabinetManagementUpdatedAsync();
    }

    public async Task UpdateCabinetAsync(Cabinet cabinet)
    {
        await _shelfRepository.UpdateCabinetAsync(cabinet);
        await _hubService.NotifyCabinetManagementUpdatedAsync();
    }

    public async Task DeleteCabinetAsync(int cabinetNumber)
    {
        await _shelfRepository.DeleteCabinetAsync(cabinetNumber);
        await _hubService.NotifyCabinetManagementUpdatedAsync();
    }

    public async Task AddRowToCabinetAsync(int cabinetNumber, Row row)
    {
        var cabinet = await _shelfRepository.GetCabinetByNumberAsync(cabinetNumber);
        if (cabinet == null) return;

        cabinet.Rows.Add(row);
        await _shelfRepository.UpdateCabinetAsync(cabinet);
        await _hubService.NotifyCabinetManagementUpdatedAsync();
    }

    public async Task RemoveRowFromCabinetAsync(int cabinetNumber, int rowNumber)
    {
        var cabinet = await _shelfRepository.GetCabinetByNumberAsync(cabinetNumber);
        if (cabinet == null) return;

        var row = cabinet.Rows.FirstOrDefault(r => r.Number == rowNumber);
        if (row == null) return;

        cabinet.Rows.Remove(row);
        await _shelfRepository.UpdateCabinetAsync(cabinet);
        await _hubService.NotifyRowRemovedAsync(cabinetNumber, rowNumber);
        await _hubService.NotifyCabinetManagementUpdatedAsync();
        await _hubService.NotifyShelfLayoutUpdatedAsync();
    }
} 