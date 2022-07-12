using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using Erm.Core;
using Erm.MessageOutbox;
using Erm.Serialization.Json;

namespace Erm.Messaging.Outbox.MySql;

internal class OutboxRepository : IOutboxRepository
{
    private readonly IClock _clock;
    private readonly string _connectionString;

    public OutboxRepository(IClock clock, string connectionString)
    {
        _clock = clock;
        _connectionString = connectionString;
    }

    private MySqlConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    public async Task<MySqlMessageOutboxEntry?> GetById(Guid entryId)
    {
        await using (var conn = CreateConnection())
        {
            await conn.OpenAsync().ConfigureAwait(false);

            await using (var command = conn.CreateCommand())
            {
                command.CommandText = "SELECT * FROM _MessageOutbox " +
                                      "WHERE Id = @Id";

                command.Parameters.AddWithValue("Id", entryId);

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync())
                    {
                        return null;
                    }

                    return new MySqlMessageOutboxEntry(
                        id: reader.GetGuid("Id"),
                        messageId: reader.GetGuid("MessageId"),
                        groupId: reader.SafeGetString("GroupId"),
                        correlationId: reader.SafeGetGuidFromString("CorrelationId"),
                        requestId: reader.SafeGetGuidFromString("RequestId"),
                        destination: reader.SafeGetString("Destination"),
                        time: reader.SafeGetDateTimeOffset("Time"),
                        timeToLive: reader.SafeGetInt32("TimeToLive"),
                        replyTo: reader.SafeGetString("ReplyTo"),
                        source: reader.SafeGetString("Source"),
                        extendedProperties: ExtendedPropertiesFromJson(reader.SafeGetString("ExtendedProperties")),
                        messageName: reader.GetString("MessageName"),
                        messageContentType: reader.GetString("MessageContentType"),
                        message: (byte[])reader.GetValue("Message"),
                        createdAt: reader.GetDateTimeOffset("CreatedAt")
                    );
                }
            }
        }
    }

    public async Task Save(IMessageOutboxEntry entry)
    {
        await using (var conn = CreateConnection())
        {
            await conn.OpenAsync().ConfigureAwait(false);
            await InsertOutbox(conn, entry);
        }

        async Task InsertOutbox(MySqlConnection conn, IMessageOutboxEntry messageOutboxEntry)
        {
            await using (var command = conn.CreateCommand())
            {
                command.CommandText = "INSERT INTO _MessageOutbox " +
                                      "(Id, MessageId, GroupId, CorrelationId, Destination, Time, TimeToLive, Source, ReplyTo, ExtendedProperties, MessageName, MessageContentType, Message,CreatedAt) " +
                                      " VALUES (" +
                                      "@Id, @MessageId, @GroupId, @CorrelationId, @Destination, @Time, @TimeToLive, @Source, @ReplyTo, @ExtendedProperties, @MessageName, @MessageContentType, @Message, @CreatedAt);";

                command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.Binary) { Value = messageOutboxEntry.Id });
                command.Parameters.Add(new MySqlParameter("@MessageId", MySqlDbType.String) { Value = messageOutboxEntry.MessageId.ToString() });
                command.Parameters.Add(new MySqlParameter("@GroupId", MySqlDbType.String) { Value = messageOutboxEntry.GroupId });
                command.Parameters.Add(new MySqlParameter("@CorrelationId", MySqlDbType.String) { Value = messageOutboxEntry.CorrelationId.ToString() });
                command.Parameters.Add(new MySqlParameter("@RequestId", MySqlDbType.String) { Value = messageOutboxEntry.RequestId.ToString() });
                command.Parameters.Add(new MySqlParameter("@Destination", MySqlDbType.String) { Value = messageOutboxEntry.Destination });
                command.Parameters.Add(new MySqlParameter("@Time", MySqlDbType.Newdate) { Value = messageOutboxEntry.Time });
                command.Parameters.Add(new MySqlParameter("@TimeToLive", MySqlDbType.Int32) { Value = messageOutboxEntry.TimeToLive });
                command.Parameters.Add(new MySqlParameter("@Source", MySqlDbType.String) { Value = messageOutboxEntry.Source });
                command.Parameters.Add(new MySqlParameter("@ReplyTo", MySqlDbType.String) { Value = messageOutboxEntry.ReplyTo });
                command.Parameters.Add(new MySqlParameter("@ExtendedProperties", MySqlDbType.JSON) { Value = ExtendedPropertiesToJson(messageOutboxEntry.ExtendedProperties) });
                command.Parameters.Add(new MySqlParameter("@MessageName", MySqlDbType.String) { Value = messageOutboxEntry.MessageName });
                command.Parameters.Add(new MySqlParameter("@MessageContentType", MySqlDbType.String) { Value = messageOutboxEntry.MessageContentType });
                command.Parameters.Add(new MySqlParameter("@Message", MySqlDbType.MediumBlob) { Value = messageOutboxEntry.Message });
                command.Parameters.Add(new MySqlParameter("@CreatedAt", MySqlDbType.Newdate) { Value = messageOutboxEntry.CreatedAt ?? _clock.UtcNow });

                await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
        }
    }

    private static string? ExtendedPropertiesToJson(Dictionary<string, string>? dictionary)
    {
        return dictionary == null || dictionary.Count == 0 ? null : JsonSerde.Serialize(dictionary);
    }

    private static Dictionary<string, string>? ExtendedPropertiesFromJson(string? json)
    {
        return string.IsNullOrWhiteSpace(json) ? null : JsonSerde.Deserialize<Dictionary<string, string>>(json);
    }
}