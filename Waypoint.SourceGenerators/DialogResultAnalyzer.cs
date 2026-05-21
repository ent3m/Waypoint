using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Waypoint.SourceGenerators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DialogResultAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "DialogResultGenerator";

    #region DESCRIPTORS
    // WYPT01: View does not inherit from Avalonia.StyledElement
    private static readonly DiagnosticDescriptor ViewNotStyledElement = new(
        id: "WYPT01",
        title: "View does not inherit from Avalonia.StyledElement",
        messageFormat: "The type '{0}' must inherit from 'Avalonia.StyledElement' or a derived type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // WYPT02: Generic View and ViewModel types are not supported
    private static readonly DiagnosticDescriptor GenericNotSupported = new(
        id: "WYPT02",
        title: "Generic classes are not supported",
        messageFormat: "The type '{0}' must be non-generic. Generic view or view model are not supported.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // WYPT03: Event accessibility is insufficient for the generated view to subscribe/unsubscribe
    private static readonly DiagnosticDescriptor EventNotAccessible = new(
        id: "WYPT03",
        title: "Event is not accessible from the view",
        messageFormat: "The event '{0}' must be internal or public for the generated view to access",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // WYPT04: Event delegate does not match the required signature
    private static readonly DiagnosticDescriptor InvalidEventSignature = new(
        id: "WYPT04",
        title: "Event uses an unsupported delegate type",
        messageFormat: "The event '{0}' must use a delegate type that returns void and accepts exactly one parameter",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [ViewNotStyledElement, GenericNotSupported, EventNotAccessible, InvalidEventSignature];
    #endregion

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Cache the attribute symbol lookup once per compilation to avoid repeated string comparisons
        context.RegisterCompilationStartAction(static compilationCtx =>
        {
            var attributeType = compilationCtx.Compilation.GetTypeByMetadataName(DialogResultGenerator.FullyQualifiedAttributeName);
            if (attributeType is not null)
                compilationCtx.RegisterSymbolAction(ctx => AnalyzeEvent(ctx, attributeType), SymbolKind.Event);
        });
    }

    private static void AnalyzeEvent(SymbolAnalysisContext ctx, INamedTypeSymbol attributeType)
    {
        if (ctx.Symbol is not IEventSymbol eventSymbol)
            return;

        // Only analyze events with the [DialogResult] attribute
        var attribute = eventSymbol.GetAttributes()
            .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
        if (attribute is null)
            return;

        var location = attribute.ApplicationSyntaxReference?.GetSyntax(ctx.CancellationToken).GetLocation()
            ?? Helper.GetSourceLocation(eventSymbol);

        // Only analyze events with a valid view type
        if (!Helper.GetViewType(attribute, out var viewType) || viewType == null)
            return;

        // WYPT01: View must inherit from Avalonia.StyledElement
        if (!Helper.IsStyledElement(viewType))
            ctx.ReportDiagnostic(Diagnostic.Create(ViewNotStyledElement, location, viewType.Name));

        // WYPT02: View must not be a generic type
        if (viewType.IsGenericType)
            ctx.ReportDiagnostic(Diagnostic.Create(GenericNotSupported, location, viewType.Name));

        // WYPT02: ViewModel must not be a generic type
        var viewModelType = eventSymbol.ContainingType;
        if (viewModelType.IsGenericType)
            ctx.ReportDiagnostic(Diagnostic.Create(GenericNotSupported, location, viewModelType.Name));

        // WYPT03: Event must be accessible from the view
        if (eventSymbol.DeclaredAccessibility < Accessibility.Internal)
            ctx.ReportDiagnostic(Diagnostic.Create(EventNotAccessible, location, eventSymbol.Name));

        // WYPT04: Delegate must return void and accept exactly one parameter
        if (!Helper.IsActionEventSignature(eventSymbol, out _))
            ctx.ReportDiagnostic(Diagnostic.Create(InvalidEventSignature, location, eventSymbol.Name));
    }
}