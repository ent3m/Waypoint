using Microsoft.CodeAnalysis;
using System.Linq;

namespace Waypoint.SourceGenerators;

internal static class Helper
{
    /// <summary>
    /// Determines whether the type derives from <c>Avalonia.StyledElement</c>.
    /// </summary>
    /// <returns>true if the type derives from <c>Avalonia.StyledElement</c>; otherwise, false.</returns>
    internal static bool IsStyledElement(INamedTypeSymbol type)
        => DerivesFrom(type, "Avalonia.StyledElement");

    /// <summary>
    /// Determines whether the type derives from <c>Avalonia.Controls.Window</c>.
    /// </summary>
    /// <returns>true if the type derives from <c>Avalonia.Controls.Window</c>; otherwise, false.</returns>
    internal static bool IsWindow(INamedTypeSymbol type)
        => DerivesFrom(type, "Avalonia.Controls.Window");

    /// <summary>
    /// Determines whether the type derives from a specified type by checking its inheritance hierarchy.
    /// </summary>
    /// <remarks>
    /// The check starts at the immediate base type, not the type itself. This is intentional: callers always pass
    /// user-defined types, which can never be a library-owned type directly.
    /// </remarks>
    /// <param name="type">The user-defined type to inspect.</param>
    /// <param name="fullyQualifiedTypeName">The fully qualified metadata name of the target base type.</param>
    /// <returns>true if the type derives from the specified type; otherwise, false.</returns>
    internal static bool DerivesFrom(INamedTypeSymbol type, string fullyQualifiedTypeName)
    {
        var currentBase = type.BaseType;
        while (currentBase != null)
        {
            if (currentBase.ToDisplayString() == fullyQualifiedTypeName)
                return true;

            currentBase = currentBase.BaseType;
        }
        return false;
    }

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