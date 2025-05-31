using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using ShelfLayout.Infrastructure.Repositories;
using ShelfLayout.Core.Interfaces;
using ShelfLayout.Web.Services;
using ShelfLayout.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.Caching.Memory;

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
builder.Services.AddScoped<ShelfLayoutHubService>();

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

await builder.Build().RunAsync();
