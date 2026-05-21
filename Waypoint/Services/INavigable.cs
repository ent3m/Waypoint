namespace Waypoint;

/// <summary>
/// Represents a ViewModel that participates in navigation lifecycle callbacks.
/// </summary>
public interface INavigable
{
    /// <summary>
    /// Invoked on the outgoing ViewModel before navigation proceeds.
    /// Return <see langword="false"/> to cancel the navigation.
    /// </summary>
    /// <remarks>
    /// Applicable to the following navigation methods:
    /// <list type="bullet">
    ///   <item><see cref="INavigator.NavigateAsync{TView}(object?, CancellationToken)"/></item>
    ///   <item><see cref="INavigator.NavigateAsync{TView, TContainer}(string?, object?, CancellationToken)"/></item>
    ///   <item><see cref="INavigator.NavigateWindowAsync{TView}(object?, CancellationToken)"/></item>
    /// </list>
    /// </remarks>
    Task<bool> OnNavigatingFromAsync(CancellationToken cancellationToken)
        => Task.FromResult(true);

    /// <summary>
    /// Invoked on the incoming ViewModel before the View is instantiated.
    /// </summary>
    Task OnNavigatingToAsync(object? parameter, CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <summary>
    /// Invoked on the incoming ViewModel after the View is fully loaded and laid out in the visual tree.
    /// </summary>
    /// <remarks>
    /// It is safe to perform UI-dependent work here, such as reading element bounds or triggering animations.
    /// Implementations should avoid synchronous blocking work, as this runs on the UI thread.
    /// </remarks>
    Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
