using Microsoft.CodeAnalysis;
using System.Linq;

namespace Waypoint.SourceGenerators;

internal static class Helper
{
    /// <summary>
    /// Determines whether the specified type represents an Avalonia StyledElement by checking its inheritance hierarchy.
    /// </summary>
    /// <remarks>
    /// The check is performed by traversing the base types of the provided symbol.</remarks>
    /// <returns>true if the type derives from Avalonia StyledElement base type; otherwise, false.</returns>
    internal static bool IsStyledElement(INamedTypeSymbol type)
    {
        var currentBase = type.BaseType;
        while (currentBase != null)
        {
            if (currentBase.ToDisplayString() == AvaloniaStyledElement)
                return true;

            currentBase = currentBase.BaseType;
        }
        return false;
    }
    private const string AvaloniaStyledElement = "Avalonia.StyledElement";

    /// <summary>
    /// Ensure void-return, single-parameter delegate signature (e.g., Action<T>)
    /// </summary>
    internal static bool IsActionEventSignature(IEventSymbol eventSymbol, out ITypeSymbol? paramType)
    {
        paramType = null;

        if (eventSymbol.Type is not INamedTypeSymbol delegateType)
            return false;

        var invokeMethod = delegateType.DelegateInvokeMethod;
        if (invokeMethod is null || !invokeMethod.ReturnsVoid || invokeMethod.Parameters.Length != 1)
            return false;

        paramType = invokeMethod.Parameters[0].Type;
        return true;
    }

    /// <summary>
    /// Get view type from the first argument of an attribute
    /// </summary>
    /// <returns>
    /// false if the argument is missing or not a type
    /// </returns>
    internal static bool GetViewType(AttributeData attribute, out INamedTypeSymbol? viewType)
    {
        viewType = null;

        if (attribute.ConstructorArguments.Length == 0)
            return false;

        if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol argType)
            return false;

        viewType = argType;
        return true;
    }

    /// <summary>
    /// Get location of the symbol in source code or the first available one
    /// </summary>
    internal static Location GetSourceLocation(ISymbol symbol)
        => symbol.Locations.FirstOrDefault(static l => l.IsInSource) ?? symbol.Locations[0];
}