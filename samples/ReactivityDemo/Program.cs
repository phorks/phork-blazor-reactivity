using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MvvmBlazor.Extensions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReactivityDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Initialize MvvmBlazor
            builder.Services.AddMvvm();

            await builder.Build().RunAsync();
        }
    }
}
