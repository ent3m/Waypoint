using Avalonia.Controls;
using Avalonia.Layout;

namespace Waypoint;

/// <summary>
/// Provides navigation and window-management operations for ViewModels.
/// </summary>
public interface INavigator
{
    /// <summary>
    /// Replaces the application's <c>MainView</c> with an instance of <typeparamref name="TView"/>.
    /// <para>Only supported under <see cref="Avalonia.Controls.ApplicationLifetimes.ISingleViewApplicationLifetime"/>.</para>
    /// </summary>
    /// <returns>
    /// A task that completes when the view is loaded.
    /// </returns>
    Task NavigateAsync<TView>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Control;

    /// <summary>
    /// Navigates to <typeparamref name="TView"/> by replacing the content of a
    /// <typeparamref name="TContainer"/> found via logical tree traversal in
    /// the <c>MainView</c> (single-view) or in any open window (desktop).
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <paramref name="containerName"/> is <see langword="null"/>, the
    /// first <typeparamref name="TContainer"/> encountered is used.
    /// When <paramref name="containerName"/> is provided, only containers whose
    /// <see cref="Avalonia.StyledElement.Name"/> matches are considered.
    /// </para>
    /// Prefer a specific <typeparamref name="TContainer"/> type over common ones. Provide
    /// <paramref name="containerName"/> when there are type collisions to avoid ambiguous matches.
    /// </remarks>
    /// <returns>
    /// A task that completes when the view is loaded.
    /// </returns>
    Task NavigateAsync<TView, TContainer>(string? containerName = null, object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Control
        where TContainer : ContentControl;

    /// <summary>
    /// Shows <typeparamref name="TView"/> as a dismissible popup rendered in the <c>OverlayLayer</c> of the visual root.
    /// </summary>
    /// <returns>
    /// A task that completes when the popup is dismissed or when <paramref name="cancellationToken"/> is canceled.
    /// </returns>
    Task ShowPopupAsync<TView>(object? parameter = null, HorizontalAlignment horizontalPlacement = HorizontalAlignment.Center,
        VerticalAlignment verticalPlacement = VerticalAlignment.Center, CancellationToken cancellationToken = default)
        where TView : Control;

    /// <summary>
    /// Shows <typeparamref name="TView"/> as a modal dialog rendered in the <c>OverlayLayer</c> of the visual root and returns the dialog result.
    /// </summary>
    /// <returns>
    /// A task that completes when the dialog is dismissed. The result is the value passed to the <see cref="IDialogView{TResult}.CloseRequested"/> event of <typeparamref name="TView"/>.
    /// </returns>
    Task<TResult> ShowDialogAsync<TView, TResult>(object? parameter = null, HorizontalAlignment horizontalPlacement = HorizontalAlignment.Center,
        VerticalAlignment verticalPlacement = VerticalAlignment.Center, CancellationToken cancellationToken = default)
        where TView : Control, IDialogView<TResult>;

    /// <summary>
    /// Replaces the application's <c>MainWindow</c> with an instance of <typeparamref name="TView"/>.
    /// <para>Only supported under <see cref="Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime"/>.</para>
    /// </summary>
    /// <returns>
    /// A task that completes when the window is loaded.
    /// </returns>
    Task NavigateWindowAsync<TView>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Window;

    /// <summary>
    /// Shows <typeparamref name="TView"/> as a window with no owner.
    /// <para>Only supported under <see cref="Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime"/>.</para>
    /// </summary>
    /// <returns>
    /// A task that completes when the window is loaded.
    /// </returns>
    Task ShowWindowAsync<TView>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Window;

    /// <summary>
    /// Shows <typeparamref name="TView"/> as a window owned by the currently open <typeparamref name="TOwner"/> window.
    /// <para>Only supported under <see cref="Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime"/>.</para>
    /// </summary>
    /// <returns>
    /// A task that completes when the window is loaded.
    /// </returns>
    Task ShowWindowAsync<TView, TOwner>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Window
        where TOwner : Window;

    /// <summary>
    /// Shows <typeparamref name="TView"/> as a dialog window owned by the currently open <typeparamref name="TOwner"/> window.
    /// <para>Only supported under <see cref="Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime"/>.</para>
    /// </summary>
    /// <returns>
    /// A task that completes when the dialog window is closed.
    /// </returns>
    Task ShowDialogWindowAsync<TView, TOwner>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Window
        where TOwner : Window;

    /// <summary>
    /// Shows <typeparamref name="TView"/> as a dialog window owned by the currently open <typeparamref name="TOwner"/> window and returns the dialog result.
    /// <para>Only supported under <see cref="Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime"/>.</para>
    /// </summary>
    /// <returns>
    /// A task that completes when the dialog window is closed. The result is the value passed to the <see cref="Window.Close(object?)"/> event of <typeparamref name="TView"/>.
    /// </returns>
    Task<TResult> ShowDialogWindowAsync<TView, TOwner, TResult>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Window
        where TOwner : Window;
}
