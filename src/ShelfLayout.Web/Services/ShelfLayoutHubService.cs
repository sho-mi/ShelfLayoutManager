using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ShelfLayout.Web.Services;

public class ShelfLayoutHubService : IShelfLayoutHubService, IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ShelfLayoutHubService> _logger;

    public event Func<Task>? OnShelfLayoutUpdated;
    public event Func<Task>? OnSkuUpdated;
    public event Func<Task>? OnCabinetManagementUpdated;
    public event Func<int, int, Task>? OnRowRemoved;
    public event Func<string, Task>? OnSkuRemoved;
    public event Func<int, int, int, Task>? OnLaneRemoved;
    public event Func<int, int, int, Task>? OnLaneAdded;

    public ShelfLayoutHubService(
        HubConnection hubConnection,
        IJSRuntime jsRuntime,
        ILogger<ShelfLayoutHubService> logger)
    {
        _hubConnection = hubConnection;
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                _logger.LogInformation("Hub connection already established");
                return;
            }

            _logger.LogInformation("Initializing hub connection");

            // Set up event handlers
            _hubConnection.On("OnShelfLayoutUpdated", async () =>
            {
                if (OnShelfLayoutUpdated != null)
                {
                    await OnShelfLayoutUpdated();
                }
            });

            _hubConnection.On("OnSkuUpdated", async () =>
            {
                if (OnSkuUpdated != null)
                {
                    await OnSkuUpdated();
                }
            });

            _hubConnection.On("OnCabinetManagementUpdated", async () =>
            {
                if (OnCabinetManagementUpdated != null)
                {
                    await OnCabinetManagementUpdated();
                }
            });

            _hubConnection.On<int, int>("OnRowRemoved", async (cabinetNumber, rowNumber) =>
            {
                if (OnRowRemoved != null)
                {
                    await OnRowRemoved(cabinetNumber, rowNumber);
                }
            });

            _hubConnection.On<string>("OnSkuRemoved", async (janCode) =>
            {
                if (OnSkuRemoved != null)
                {
                    await OnSkuRemoved(janCode);
                }
            });

            _hubConnection.On<int, int, int>("OnLaneRemoved", async (cabinetNumber, rowNumber, laneNumber) =>
            {
                if (OnLaneRemoved != null)
                {
                    await OnLaneRemoved(cabinetNumber, rowNumber, laneNumber);
                }
            });

            _hubConnection.On<int, int, int>("OnLaneAdded", async (cabinetNumber, rowNumber, laneNumber) =>
            {
                if (OnLaneAdded != null)
                {
                    await OnLaneAdded(cabinetNumber, rowNumber, laneNumber);
                }
            });

            // Start the connection
            await _hubConnection.StartAsync();
            _logger.LogInformation("Hub connection established successfully");

            _hubConnection.Closed += async (error) =>
            {
                _logger.LogWarning(error, "Hub connection closed");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                try
                {
                    await _hubConnection.StartAsync();
                    _logger.LogInformation("Hub connection re-established");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to re-establish hub connection");
                }
            };

            _hubConnection.Reconnecting += (error) =>
            {
                _logger.LogWarning(error, "Hub connection reconnecting");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                _logger.LogInformation("Hub connection reconnected with ID: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize hub connection");
            // Don't throw the exception, just log it
        }
    }

    public async Task NotifyShelfLayoutUpdatedAsync()
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifyShelfLayoutUpdated");
        }
    }

    public async Task NotifySkuUpdatedAsync()
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifySkuUpdated");
        }
    }

    public async Task NotifyCabinetManagementUpdatedAsync()
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifyCabinetManagementUpdated");
        }
    }

    public async Task NotifyRowRemovedAsync(int cabinetNumber, int rowNumber)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifyRowRemoved", cabinetNumber, rowNumber);
        }
    }

    public async Task NotifySkuRemovedAsync(string janCode)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifySkuRemoved", janCode);
        }
    }

    public async Task NotifyLaneRemovedAsync(int cabinetNumber, int rowNumber, int laneNumber)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifyLaneRemoved", cabinetNumber, rowNumber, laneNumber);
        }
    }

    public async Task NotifyLaneAddedAsync(int cabinetNumber, int rowNumber, int laneNumber)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifyLaneAdded", cabinetNumber, rowNumber, laneNumber);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
} 