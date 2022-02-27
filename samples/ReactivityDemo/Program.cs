using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MvvmBlazor.Extensions;
using Phork.Blazor;
using ReactivityDemo;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddPhorkBlazorReactivity();

// Register MvvmBlazor
builder.Services.AddMvvm();

await builder.Build().RunAsync();