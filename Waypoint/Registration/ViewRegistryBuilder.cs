using Avalonia.Controls;
using System.Collections.Frozen;

namespace Waypoint;

internal sealed class ViewRegistryBuilder : IViewRegistry
{
    private readonly Dictionary<Type, Type> _entries = [];

    public IViewRegistry Register<TView, TViewModel>()
        where TView : Control
    {
        _entries[typeof(TView)] = typeof(TViewModel);
        return this;
    }

    internal FrozenDictionary<Type, Type> Build() => _entries.ToFrozenDictionary();
}
