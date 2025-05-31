using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ShelfLayout.Web.Services;

public class ShelfLayoutHubService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly IJSRuntime _jsRuntime;
    private readonly string _hubUrl;
    private readonly ILogger<ShelfLayoutHubService> _logger;

    public event Func<Task>? OnShelfLayoutUpdated;
    public event Func<Task>? OnSkuUpdated;
    public event Func<Task>? OnCabinetManagementUpdated;
    public event Func<int, int, Task>? OnRowRemoved;
    public event Func<string, Task>? OnSkuRemoved;

    public ShelfLayoutHubService(IJSRuntime jsRuntime, IConfiguration configuration, ILogger<ShelfLayoutHubService> logger)
    {
        _jsRuntime = jsRuntime;
        _hubUrl = configuration["HubUrl"] ?? "https://localhost:5237/shelflayouthub";
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                _logger.LogInformation("Hub connection already established");
                return;
            }

            _logger.LogInformation("Initializing hub connection");
            
            // Create the hub connection if it doesn't exist
            if (_hubConnection == null)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                    .Build();

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
            }

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
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifyShelfLayoutUpdated");
        }
    }

    public async Task NotifySkuUpdatedAsync()
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifySkuUpdated");
        }
    }

    public async Task NotifyCabinetManagementUpdatedAsync()
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifyCabinetManagementUpdated");
        }
    }

    public async Task NotifyRowRemovedAsync(int cabinetNumber, int rowNumber)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifyRowRemoved", cabinetNumber, rowNumber);
        }
    }

    public async Task NotifySkuRemovedAsync(string janCode)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("NotifySkuRemoved", janCode);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
} 