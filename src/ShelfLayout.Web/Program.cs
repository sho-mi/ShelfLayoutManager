using ShelfLayout.Infrastructure.Repositories;
using ShelfLayout.Web;
using Polly;
using Microsoft.AspNetCore.SignalR.Client;
using ShelfLayout.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Get the API URL from configuration
var apiUrl = builder.Configuration["ApiUrl"] ?? "https://localhost:5237";

// Add memory cache
builder.Services.AddMemoryCache();

// Configure HttpClient with the correct base address and retry policy
builder.Services.AddHttpClient("ShelfLayoutAPI", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddTransientHttpErrorPolicy(policy => 
    policy.WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// Register repositories
builder.Services.AddScoped<ISkuRepository, SkuCsvRepository>();
builder.Services.AddScoped<IShelfRepository, JsonShelfRepository>();

// Register services
builder.Services.AddScoped<ShelfLayoutService>();

// Configure SignalR
builder.Services.AddSingleton<HubConnection>(sp =>
{
    var hubUrl = builder.Configuration["HubUrl"] ?? $"{apiUrl}/shelflayouthub";
    return new HubConnectionBuilder()
        .WithUrl(hubUrl, options =>
        {
            options.SkipNegotiation = false;
            options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | 
                               Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
        })
        .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
        .Build();
});

// Register the hub service
builder.Services.AddScoped<IShelfLayoutHubService, ShelfLayoutHubService>();

await builder.Build().RunAsync();
