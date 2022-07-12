using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class EnvelopeProperties : Dictionary<string, string>
{
    public EnvelopeProperties() : base(StringComparer.InvariantCultureIgnoreCase)
    {
    }

    public EnvelopeProperties(IDictionary<string, string> dictionary) : base(dictionary, StringComparer.InvariantCultureIgnoreCase)
    {
    }

    public string? Get(string key)
    {
        TryGetValue(key, out var value);
        return value;
    }

    public void Set(string key, string value)
    {
        this[key] = value;
    }
}