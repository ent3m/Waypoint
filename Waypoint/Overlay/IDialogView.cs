namespace Waypoint;

/// <summary>
/// Represents a dialog view that can request a typed close result.
/// </summary>
public interface IDialogView<TResult>
{
    /// <summary>
    /// Raised to signal that the dialog should close with the given result.
    /// </summary>
    event Action<TResult>? CloseRequested;
}
