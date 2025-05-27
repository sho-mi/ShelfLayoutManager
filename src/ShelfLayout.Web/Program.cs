using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShelfLayout.Infrastructure.Repositories;
using ShelfLayout.Core.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register repositories
builder.Services.AddScoped<ISkuRepository, SkuCsvRepository>();
builder.Services.AddScoped<IShelfRepository, JsonShelfRepository>();

// Register services
builder.Services.AddScoped<ShelfLayoutService>();

await builder.Build().RunAsync();
