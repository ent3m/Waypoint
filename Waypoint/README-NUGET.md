Waypoint is a View-first, type-safe navigation framework for Avalonia with native DI support.

## Features

- **Type Safety**: Navigate by View with compile-time enforcement. No regions, no magic strings.
- **DI-Native**: Views and ViewModels are resolved directly from the DI container.
- **Flexible Routing**: Navigate anywhere — swap views, open windows, or replace the main window.
- **Lifecycle Management**: Full control over transitions with data passing, guard, and hooks via [`INavigable`](#lifecycle-management).
- **Built-in Dialog**: Modal dialogs and popup toasts with source-generated implementations.
- **Native AOT**: Native AOT compilation and trimming support.
- **MVVM**: Navigation targets are passed as types, leaving ViewModels entirely decoupled from Views. Works with any MVVM framework.

## Quick Start

1. Register navigation in your DI Container:
```C#
public override void OnFrameworkInitializationCompleted()
{
    var services = new ServiceCollection();
    services.AddNavigation(ApplicationLifetime, RegisterViews);
    // configure views, view models, and other services
}

// Map views to view models
private static void RegisterViews(IViewRegistry views) => views
    .Register<HomeView, HomeViewModel>()
    .Register<SettingsView, SettingsViewModel>();
```
2. Inject `INavigator` and navigate:
```C#
public class HomeViewModel(INavigator navigator)
{
    public async Task GoToSettingsAsync()
    {
        await navigator.NavigateAsync<SettingsView>();
    }
}
```