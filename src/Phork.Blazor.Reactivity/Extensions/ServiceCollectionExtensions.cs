using Microsoft.Extensions.DependencyInjection;
using Phork.Blazor.Services;

namespace Phork.Blazor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPhorkBlazorReactivity(this IServiceCollection services)
    {
        services.AddTransient<IPropertyObserver, PropertyObserver>();
        services.AddTransient<ICollectionObserver, CollectionObserver>();

        services.AddTransient<IReactivityManager, ReactivityManager>();

        return services;
    }
}