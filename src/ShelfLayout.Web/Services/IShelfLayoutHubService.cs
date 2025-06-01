using System;
using System.Threading.Tasks;

namespace ShelfLayout.Web.Services;

public interface IShelfLayoutHubService : IAsyncDisposable
{
    event Func<Task>? OnShelfLayoutUpdated;
    event Func<Task>? OnSkuUpdated;
    event Func<Task>? OnCabinetManagementUpdated;
    event Func<int, int, Task>? OnRowRemoved;
    event Func<string, Task>? OnSkuRemoved;
    event Func<int, int, int, Task>? OnLaneRemoved;
    event Func<int, int, int, Task>? OnLaneAdded;

    Task InitializeAsync();
    Task NotifyShelfLayoutUpdatedAsync();
    Task NotifySkuUpdatedAsync();
    Task NotifyCabinetManagementUpdatedAsync();
    Task NotifyRowRemovedAsync(int cabinetNumber, int rowNumber);
    Task NotifySkuRemovedAsync(string janCode);
    Task NotifyLaneRemovedAsync(int cabinetNumber, int rowNumber, int laneNumber);
    Task NotifyLaneAddedAsync(int cabinetNumber, int rowNumber, int laneNumber);
} 