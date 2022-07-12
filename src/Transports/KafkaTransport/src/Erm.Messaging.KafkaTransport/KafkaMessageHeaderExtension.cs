using System;
using Erm.KafkaClient;
using Erm.Serialization.Json;

namespace Erm.Messaging.KafkaTransport;

internal static class KafkaMessageHeaderExtension
{
    public static void AddStr(this IKafkaMessageHeaders headers, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            headers.AddString(name, value);
        }
    }

    public static Guid? GetGuid(this IKafkaMessageHeaders headers, string name)
    {
        var stringValue = headers.GetString(name);
        return !string.IsNullOrWhiteSpace(stringValue) && Guid.TryParse(stringValue, out var value)
            ? value
            : null;
    }

    public static int? GetInt(this IKafkaMessageHeaders headers, string name)
    {
        var stringValue = headers.GetString(name);
        return !string.IsNullOrWhiteSpace(stringValue) && int.TryParse(stringValue, out var value)
            ? value
            : null;
    }

    public static string? GetStr(this IKafkaMessageHeaders headers, string name)
    {
        return headers.GetString(name);
    }

    public static DateTimeOffset? GetDateTimeOffset(this IKafkaMessageHeaders headers, string name)
    {
        var stringValue = headers.GetString(name);
        return !string.IsNullOrWhiteSpace(stringValue) && DateTimeOffset.TryParse(stringValue, out var dateTime)
            ? dateTime
            : null;
    }

    public static EnvelopeProperties? GetExtendedProperties(this IKafkaMessageHeaders headers, string name)
    {
        var stringValue = headers.GetString(name);
        return string.IsNullOrWhiteSpace(stringValue) ? null : JsonSerde.Deserialize<EnvelopeProperties>(stringValue);
    }
}