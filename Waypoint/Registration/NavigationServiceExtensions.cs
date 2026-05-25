using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Waypoint;

/// <summary>
/// Extension methods for registering the Waypoint navigation framework with <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>.
/// </summary>
public static class NavigationServiceExtensions
{
    /// <summary>
    /// Registers the navigation framework and its View-to-ViewModel mappings in the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="applicationLifetime">The current application lifetime used by navigation.</param>
    /// <param name="configureMappings">A callback used to register View-to-ViewModel mappings.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddNavigation(this IServiceCollection services, IApplicationLifetime? applicationLifetime, Action<IViewRegistry> configureMappings)
    {
        ArgumentNullException.ThrowIfNull(configureMappings, nameof(configureMappings));

        var builder = new ViewRegistryBuilder();
        configureMappings(builder);
        var mappings = builder.Build();

        return services.AddSingleton<INavigator>(sp =>
            new Navigator(sp, Dispatcher.UIThread, applicationLifetime, mappings));
    }
}
