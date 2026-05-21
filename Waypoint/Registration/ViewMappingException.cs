namespace Waypoint;

/// <summary>
/// Thrown when navigation cannot resolve a viewmodel mapping for a view type.
/// </summary>
public sealed class ViewMappingException(Type viewType) : Exception(
    $"No ViewModel has been registered for the View: {viewType.Name}. Ensure you called Register<TView, TViewModel>() during initialization.")
{
    public Type ViewType { get; } = viewType;
}
