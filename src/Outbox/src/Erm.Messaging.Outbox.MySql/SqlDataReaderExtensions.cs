using System;
using MySqlConnector;

namespace Erm.Messaging.Outbox.MySql;

internal static class SqlDataReaderExtensions
{
    internal static string? SafeGetString(this MySqlDataReader reader, string name)
    {
        return reader.IsDBNull(reader.GetOrdinal(name)) ? null : reader.GetString(name);
    }

    internal static DateTimeOffset? SafeGetDateTimeOffset(this MySqlDataReader reader, string name)
    {
        return reader.IsDBNull(reader.GetOrdinal(name)) ? null : reader.GetDateTimeOffset(name);
    }

    internal static int? SafeGetInt32(this MySqlDataReader reader, string name)
    {
        return reader.IsDBNull(reader.GetOrdinal(name)) ? null : reader.GetInt32(name);
    }

    internal static Guid? SafeGetGuidFromString(this MySqlDataReader reader, string name)
    {
        var guid = reader.SafeGetString(name);
        return string.IsNullOrEmpty(guid) ? null : Guid.Parse(guid);
    }
}