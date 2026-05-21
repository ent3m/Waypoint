using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Waypoint;

/// <summary>
/// A full-size overlay panel placed in the <c>OverlayLayer</c> to host a dismissible popup.
/// It listens for pointer events and Escape key down events for dismissal.
/// It absorbs those events to prevent accidental activation of controls behind the popup.
/// </summary>
internal sealed class PopupOverlayHost : Panel
{
    private readonly Border _contentHost;
    private readonly TaskCompletionSource _dismissedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal Task DismissTask => _dismissedTcs.Task;

    public PopupOverlayHost(Control content, HorizontalAlignment horizontalPlacement, VerticalAlignment verticalPlacement)
    {
        this.Focusable = true; // Enable focus to receive key events

        Background = Brushes.Transparent;

        _contentHost = new Border
        {
            Child = content,
            HorizontalAlignment = horizontalPlacement,
            VerticalAlignment = verticalPlacement,
        };

        Children.Add(_contentHost);

        AddHandler(PointerPressedEvent, OnPointerPressed, Avalonia.Interactivity.RoutingStrategies.Bubble);
        AddHandler(KeyDownEvent, OnKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }

    // Dismiss the popup when the user clicks anywhere on the overlay.
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
        Dismiss();
    }

    // Dismiss the popup when the user presses the Escape key.
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Dismiss();
        }
    }

    internal void Dismiss() => _dismissedTcs.TrySetResult();

    protected override Size MeasureOverride(Size availableSize)
    {
        _contentHost.Measure(availableSize);
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _contentHost.Arrange(new Rect(finalSize));
        return finalSize;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        this.Focus();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _contentHost.Child = null;
        base.OnDetachedFromVisualTree(e);
    }
}
