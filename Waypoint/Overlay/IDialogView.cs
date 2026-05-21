namespace Waypoint;

/// <summary>
/// Represents a dialog view that can request a typed close result.
/// </summary>
public interface IDialogView<TResult>
{
    event Action<TResult>? CloseRequested;
}
