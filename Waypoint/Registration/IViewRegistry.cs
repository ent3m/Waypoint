using Avalonia.Controls;

namespace Waypoint;

/// <summary>
/// Registers View and ViewModel type mappings used by the navigation system.
/// </summary>
public interface IViewRegistry
{
    /// <summary>
    /// Registers a View type and its corresponding ViewModel type.
    /// </summary>
    /// <typeparam name="TView">The View type.</typeparam>
    /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
    /// <returns>The registry instance for chaining.</returns>
    IViewRegistry Register<TView, TViewModel>()
        where TView : Control;
}
