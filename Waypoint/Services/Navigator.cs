using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;

namespace Waypoint;

/// <summary>
/// Implementation of <see cref="INavigator"/> that resolves Views and ViewModels
/// from the DI container and dispatches all UI work to the Avalonia UI thread.
/// </summary>
/// <remarks>
/// <para>
/// Supports both <see cref="IClassicDesktopStyleApplicationLifetime"/> and <see cref="ISingleViewApplicationLifetime"/>.
/// Desktop lifetime leverages Avalonia Window API (<c>Window.Show</c>, <c>Window.ShowDialog</c>).
/// Overlay APIs (<c>ShowPopup</c>, <c>ShowDialog</c>) work in both lifetimes by locating the
/// <see cref="Avalonia.Controls.Primitives.OverlayLayer"/> of the active visual root.
/// </para>
/// <para>
/// Before navigation begins, <see cref="INavigable.OnNavigatingFromAsync(CancellationToken)"/> is called if applicable.
/// The navigation can be cancelled at this point by the outgoing ViewModel returning <see langword="false"/>.
/// Every navigation method eventually calls <see cref="GetViewWithDataContextAsync{TView}"/>, 
/// which performs the following steps in order:
/// <list type="number">
///   <item>Looks up the registered ViewModel type for the View in the registry.</item>
///   <item>Resolves the View and ViewModel from the DI container.</item>
///   <item>Calls <see cref="INavigable.OnNavigatingToAsync"/> on the ViewModel if it implements
///     <see cref="INavigable"/>, so it can prepare state before the View is created.</item>
///   <item>Resolves the View from the DI container and assigns the ViewModel as its <c>DataContext</c>.</item>
/// </list>
/// Shows the View by locating the named <c>ContentControl</c> or by directly assigning <c>MainWindow</c> or <c>MainView</c>.
/// After the View is shown, <see cref="INavigable.OnNavigatedToAsync"/> is called if applicable.
/// </para>
/// </remarks>
internal sealed class Navigator : INavigator
{
    private readonly Dispatcher _dispatcher;
    private readonly IServiceProvider _serviceProvider;
    private readonly IClassicDesktopStyleApplicationLifetime? _desktop;
    private readonly ISingleViewApplicationLifetime? _single;
    private readonly FrozenDictionary<Type, Type> _mappings;

    public Navigator(IServiceProvider serviceProvider, Dispatcher dispatcher, IApplicationLifetime? appLifetime, FrozenDictionary<Type, Type> mappings)
    {
        ArgumentNullException.ThrowIfNull(appLifetime);

        if (appLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            _desktop = desktop;
        else if (appLifetime is ISingleViewApplicationLifetime single)
            _single = single;
        else
            throw new ArgumentException("Only IClassicDesktopStyleApplicationLifetime and ISingleViewApplicationLifetime are supported by INavigator.", nameof(appLifetime));

        _serviceProvider = serviceProvider;
        _dispatcher = dispatcher;
        _mappings = mappings;
    }

    public Task NavigateWindowAsync<TView>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Window
    {
        if (_desktop is null)
            throw new InvalidOperationException("This operation is only supported in applications using IClassicDesktopStyleApplicationLifetime.");

        return _dispatcher.CheckAccess() ? CoreAsync() : _dispatcher.InvokeAsync(CoreAsync);

        async Task CoreAsync()
        {
            INavigable? outgoingViewModel = _desktop.MainWindow?.DataContext as INavigable;
            if (outgoingViewModel is not null && !await outgoingViewModel.OnNavigatingFromAsync(cancellationToken))
                return;  // Navigation cancelled by the outgoing ViewModel

            var (window, navigable) = await GetViewWithDataContextAsync<TView>(parameter, cancellationToken);
            var prevWindow = _desktop.MainWindow;
            // Show the new window before closing the previous window to keep the application alive in case there's no other open window.
            window.Show();
            _desktop.MainWindow = window;
            prevWindow?.Close();
            if (navigable is not null)
            {
                // Wait for the window to be fully loaded and laid out before invoking OnNavigatedToAsync,
                // so that any UI-dependent code in the ViewModel runs against a fully constructed visual tree.
                await AwaitLoadedAsync(window, cancellationToken);
                await navigable.OnNavigatedToAsync(parameter, cancellationToken);
            }
        }
    }

    public Task ShowWindowAsync<TView>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Window
    {
        if (_desktop is null)
            throw new InvalidOperationException("This operation is only supported in applications using IClassicDesktopStyleApplicationLifetime.");

        return _dispatcher.CheckAccess() ? CoreAsync() : _dispatcher.InvokeAsync(CoreAsync);

        async Task CoreAsync()
        {
            var (window, navigable) = await GetViewWithDataContextAsync<TView>(parameter, cancellationToken);
            window.Show();
            if (navigable is not null)
            {
                await AwaitLoadedAsync(window, cancellationToken);
                await navigable.OnNavigatedToAsync(parameter, cancellationToken);
            }
        }
    }

    public Task ShowWindowAsync<TView, TOwner>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Window
        where TOwner : Window
    {
        return _dispatcher.CheckAccess() ? CoreAsync() : _dispatcher.InvokeAsync(CoreAsync);

        async Task CoreAsync()
        {
            var owner = FindActiveWindow<TOwner>();
            var (window, navigable) = await GetViewWithDataContextAsync<TView>(parameter, cancellationToken);
            window.Show(owner);
            if (navigable is not null)
            {
                await AwaitLoadedAsync(window, cancellationToken);
                await navigable.OnNavigatedToAsync(parameter, cancellationToken);
            }
        }
    }

    public Task ShowDialogWindowAsync<TView, TOwner>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Window
        where TOwner : Window
    {
        return _dispatcher.CheckAccess() ? CoreAsync() : _dispatcher.InvokeAsync(CoreAsync);

        async Task CoreAsync()
        {
            var owner = FindActiveWindow<TOwner>();
            var (window, navigable) = await GetViewWithDataContextAsync<TView>(parameter, cancellationToken);
            Task dialog = window.ShowDialog(owner);
            if (navigable is not null)
            {
                await AwaitLoadedAsync(window, cancellationToken);
                await navigable.OnNavigatedToAsync(parameter, cancellationToken);
            }
            await dialog;
        }
    }

    public Task<TResult> ShowDialogWindowAsync<TView, TOwner, TResult>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Window
        where TOwner : Window
    {
        return _dispatcher.CheckAccess() ? CoreAsync() : _dispatcher.InvokeAsync(CoreAsync);

        async Task<TResult> CoreAsync()
        {
            var owner = FindActiveWindow<TOwner>();
            var (window, navigable) = await GetViewWithDataContextAsync<TView>(parameter, cancellationToken);
            Task<TResult> dialog = window.ShowDialog<TResult>(owner);
            if (navigable is not null)
            {
                await AwaitLoadedAsync(window, cancellationToken);
                await navigable.OnNavigatedToAsync(parameter, cancellationToken);
            }
            return await dialog;
        }
    }

    public Task NavigateAsync<TView>(object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Control
    {
        if (_single is null)
            throw new InvalidOperationException("This operation is only supported in applications using ISingleViewApplicationLifetime.");

        return _dispatcher.CheckAccess() ? CoreAsync() : _dispatcher.InvokeAsync(CoreAsync);

        async Task CoreAsync()
        {
            INavigable? outgoingViewModel = _single.MainView?.DataContext as INavigable;
            if (outgoingViewModel is not null && !await outgoingViewModel.OnNavigatingFromAsync(cancellationToken))
                return;  // Navigation was cancelled by the outgoing ViewModel
            var (view, navigable) = await GetViewWithDataContextAsync<TView>(parameter, cancellationToken);
            _single.MainView = view;
            if (navigable is not null)
            {
                await AwaitLoadedAsync(view, cancellationToken);
                await navigable.OnNavigatedToAsync(parameter, cancellationToken);
            }
        }
    }

    public Task NavigateAsync<TView, TContainer>(string? containerName = null, object? parameter = null, CancellationToken cancellationToken = default)
        where TView : Control
        where TContainer : ContentControl
    {
        return _dispatcher.CheckAccess() ? CoreAsync() : _dispatcher.InvokeAsync(CoreAsync);

        async Task CoreAsync()
        {
            var container = FindActiveControl<TContainer>(containerName);
            INavigable? outgoingViewModel = (container.Content as StyledElement)?.DataContext as INavigable;
            if (outgoingViewModel is not null && !await outgoingViewModel.OnNavigatingFromAsync(cancellationToken))
                return;  // Navigation was cancelled by the outgoing ViewModel
            var (view, navigable) = await GetViewWithDataContextAsync<TView>(parameter, cancellationToken);
            container.Content = view;
            if (navigable is not null)
            {
                await AwaitLoadedAsync(view, cancellationToken);
                await navigable.OnNavigatedToAsync(parameter, cancellationToken);
            }
        }
    }

    public Task ShowPopupAsync<TView>(
        object? parameter = null,
        HorizontalAlignment horizontalPlacement = HorizontalAlignment.Center,
        VerticalAlignment verticalPlacement = VerticalAlignment.Center,
        CancellationToken cancellationToken = default)
        where TView : Control
    {
        return _dispatcher.CheckAccess() ? CoreAsync() : _dispatcher.InvokeAsync(CoreAsync);

        async Task CoreAsync()
        {
            var overlayLayer = GetOverlayLayer();
            var (view, navigable) = await GetViewWithDataContextAsync<TView>(parameter, cancellationToken);
            var host = new PopupOverlayHost(view, horizontalPlacement, verticalPlacement);
            await using var cancellationRegistration = cancellationToken.Register(() => host.Dismiss());
            overlayLayer.Children.Add(host);
            if (navigable is not null)
            {
                await AwaitLoadedAsync(view, cancellationToken);
                await navigable.OnNavigatedToAsync(parameter, cancellationToken);
            }
            await host.DismissTask;
            RemoveHostFromOverlay(overlayLayer, host);
        }
    }

    public Task<TResult> ShowDialogAsync<TView, TResult>(
        object? parameter = null,
        HorizontalAlignment horizontalPlacement = HorizontalAlignment.Center,
        VerticalAlignment verticalPlacement = VerticalAlignment.Center,
        CancellationToken cancellationToken = default)
        where TView : Control, IDialogView<TResult>
    {
        return _dispatcher.CheckAccess() ? CoreAsync() : _dispatcher.InvokeAsync(CoreAsync);

        async Task<TResult> CoreAsync()
        {
            var overlayLayer = GetOverlayLayer();
            var (view, navigable) = await GetViewWithDataContextAsync<TView>(parameter, cancellationToken);
            var host = new DialogOverlayHost(view, horizontalPlacement, verticalPlacement);
            var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Unsubscribe from the CloseRequested event and remove the host from the overlay
            // when the dialog is closed, either through the CloseRequested event or cancellation.
            void OnCloseRequested(TResult result)
            {
                view.CloseRequested -= OnCloseRequested;
                RemoveHostFromOverlay(overlayLayer, host);
                tcs.TrySetResult(result);
            }
            view.CloseRequested += OnCloseRequested;
            using var reg = cancellationToken.Register(() =>
            {
                view.CloseRequested -= OnCloseRequested;
                RemoveHostFromOverlay(overlayLayer, host);
                tcs.TrySetCanceled(cancellationToken);
            });

            overlayLayer.Children.Add(host);
            if (navigable is not null)
            {
                await AwaitLoadedAsync(view, cancellationToken);
                await navigable.OnNavigatedToAsync(parameter, cancellationToken);
            }

            return await tcs.Task;
        }
    }

    private async Task<(TView View, INavigable? Navigable)> GetViewWithDataContextAsync<TView>(object? parameter, CancellationToken cancellationToken)
        where TView : StyledElement
    {
        if (!_mappings.TryGetValue(typeof(TView), out var viewModelType))
            throw new ViewMappingException(typeof(TView));

        var viewModel = _serviceProvider.GetRequiredService(viewModelType);

        // Invoke OnNavigatingToAsync before creating the View so the ViewModel can load
        // its initial state (e.g., fetch data) before the bindings are evaluated.
        var navigable = viewModel as INavigable;
        if (navigable is not null)
            await navigable.OnNavigatingToAsync(parameter, cancellationToken);

        var view = _serviceProvider.GetRequiredService<TView>();
        view.DataContext = viewModel;
        return (view, navigable);
    }

    private Window FindActiveWindow<T>()
        where T : Window
    {
        if (_desktop is null)
            throw new InvalidOperationException("This operation is only supported in applications using IClassicDesktopStyleApplicationLifetime.");

        var window = _desktop.Windows.FirstOrDefault(window => window is T);
        return window ?? throw new InvalidOperationException($"No window of type {typeof(T).FullName} is currently open.");
    }

    private ContentControl FindActiveControl<T>(string? name)
        where T : ContentControl
    {
        ContentControl? control = null;

        if (_single?.MainView is not null)
            control = FindControl(_single.MainView);
        else if (_desktop is not null)
            control = FindControlInWindows(_desktop.Windows);

        return control ?? throw new InvalidOperationException(
            string.Format("No control {0} was found in any open window or main view.",
            string.IsNullOrEmpty(name)
            ? $"of type {typeof(T).FullName}"
            : $"of type {typeof(T).FullName} with name '{name}'"));

        ContentControl? FindControlInWindows(IEnumerable<Window> windows)
        {
            foreach (var window in windows)
            {
                var control = FindControl(window);
                if (control is not null)
                    return control;
            }
            return null;
        }

        ContentControl? FindControl(ILogical root)
            => root
            .GetSelfAndLogicalDescendants()
            .OfType<T>()
            .FirstOrDefault(x => string.IsNullOrEmpty(name) || x.Name == name);
    }

    private OverlayLayer GetOverlayLayer()
    {
        OverlayLayer? layer = null;

        if (_single?.MainView is Control singleRoot)
            layer = OverlayLayer.GetOverlayLayer(singleRoot);
        else if (_desktop?.MainWindow is Window desktopRoot)
            layer = OverlayLayer.GetOverlayLayer(desktopRoot);

        return layer ?? throw new InvalidOperationException(
            "No OverlayLayer was found on the current visual root. " +
            "Ensure the visual tree is fully initialized before calling overlay navigation methods, " +
            "and that the root control is hosted inside a VisualLayerManager.");
    }

    private static void RemoveHostFromOverlay(OverlayLayer overlayLayer, Control host)
        => overlayLayer.Children.Remove(host);

    private static Task AwaitLoadedAsync(Control control, CancellationToken cancellationToken)
    {
        if (control.IsLoaded)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Unsubscribe from the Loaded event and dispose the CancellationTokenRegistration
        // when the control is loaded or when cancellation is requested.
        // Declare reg before subscribing to Loaded so it can be captured by closure in both callbacks.
        // By the time either callback executes, reg will have been assigned its real value.
        CancellationTokenRegistration reg = default;
        void OnLoaded(object? s, RoutedEventArgs e)
        {
            reg.Dispose();
            control.Loaded -= OnLoaded;
            tcs.TrySetResult();
        }
        control.Loaded += OnLoaded;
        reg = cancellationToken.Register(() =>
        {
            // Self-dispose is safe. CancellationTokenRegistration correctly handles
            // the degenerate case where the callback is unregistering itself.
            reg.Dispose();
            control.Loaded -= OnLoaded;
            tcs.TrySetCanceled(cancellationToken);
        });
        return tcs.Task;
    }
}
