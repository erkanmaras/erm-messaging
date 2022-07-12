using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class ContextProperties
{
    private readonly IDictionary<string, object> _values;

    public ContextProperties()
    {
        _values = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
    }

    public void Set(string key, object value)
    {
        _values[key] = value;
    }

    public object? Get(string key)
    {
        _values.TryGetValue(key, out var value);
        return value;
    }

    public IReadOnlyDictionary<string, object> GetValues()
    {
        return new ReadOnlyDictionary<string, object>(_values);
    }
}