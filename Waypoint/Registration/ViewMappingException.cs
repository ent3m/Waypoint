namespace Waypoint;

/// <summary>
/// Thrown when navigation cannot resolve a viewmodel mapping for a view type.
/// </summary>
public sealed class ViewMappingException(Type viewType) : Exception(
    $"No ViewModel has been registered for the View: {viewType.Name}. Ensure you registered View-to-ViewModel mappings during navigation service registration.")
{
    /// <summary>
    /// Gets the view type that has not been mapped to a corresponding ViewModel.
    /// </summary>
    public Type ViewType { get; } = viewType;
}
