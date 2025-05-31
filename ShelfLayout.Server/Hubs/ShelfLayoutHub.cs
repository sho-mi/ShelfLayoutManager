using Microsoft.AspNetCore.SignalR;

namespace ShelfLayout.Server.Hubs;

public interface IShelfLayoutHub
{
    Task OnSkuRemoved(string janCode);
    Task OnSkuAdded(string janCode);
    Task OnRowRemoved(int cabinetNumber, int rowNumber);
    Task OnShelfLayoutUpdated();
}

public class ShelfLayoutHub : Hub<IShelfLayoutHub>
{
    public async Task NotifySkuRemoved(string janCode)
    {
        await Clients.All.OnSkuRemoved(janCode);
    }

    public async Task NotifySkuAdded(string janCode)
    {
        await Clients.All.OnSkuAdded(janCode);
    }

    public async Task NotifyRowRemoved(int cabinetNumber, int rowNumber)
    {
        await Clients.All.OnRowRemoved(cabinetNumber, rowNumber);
    }

    public async Task NotifyShelfLayoutUpdated()
    {
        await Clients.All.OnShelfLayoutUpdated();
    }
} 