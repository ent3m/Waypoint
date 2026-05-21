using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;

namespace Waypoint;

/// <summary>
/// A full-size overlay panel placed in the <c>OverlayLayer</c> to host a blocking content dialog.
/// </summary>
internal sealed class DialogOverlayHost : Panel
{
    private readonly Border _dimLayer;
    private readonly Border _contentHost;

    public DialogOverlayHost(Control content, HorizontalAlignment horizontalPlacement, VerticalAlignment verticalPlacement)
    {
        // Provide a semi-transparent layer that dims the content behind the dialog and blocks pointer events.
        // Set the fallback brush at Style priority so the dynamic resource takes precedence when defined,
        // but the default #99000000 is used when WaypointDialogDimBrush is absent.
        _dimLayer = new Border();
        _dimLayer.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(0x99, 0x00, 0x00, 0x00)), Avalonia.Data.BindingPriority.Style);
        _dimLayer.Bind(Border.BackgroundProperty, new DynamicResourceExtension("WaypointDialogDimBrush"));

        _contentHost = new Border
        {
            Child = content,
            HorizontalAlignment = horizontalPlacement,
            VerticalAlignment = verticalPlacement,
        };

        Children.Add(_dimLayer);
        Children.Add(_contentHost);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _dimLayer.Measure(availableSize);
        _contentHost.Measure(availableSize);
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var fullRect = new Rect(finalSize);
        // Both children are given the full rect; the dim layer stretches to fill it,
        // while the content host uses its alignment to position itself within it.
        _dimLayer.Arrange(fullRect);
        _contentHost.Arrange(fullRect);
        return finalSize;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _contentHost.Child = null;
        base.OnDetachedFromVisualTree(e);
    }
}
