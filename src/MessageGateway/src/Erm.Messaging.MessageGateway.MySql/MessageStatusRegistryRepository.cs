using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using MySqlConnector;
using Erm.Core;
using Erm.Messaging.MessageGateway;
using Erm.Serialization.Json;

namespace Erm.Messaging.MessageGateway.MySql;

internal class MessageStatusRegistryRepository
{
    private readonly IMessageStatusRegistryMySqlConfiguration _configuration;
    private readonly IClock _clock;

    public MessageStatusRegistryRepository(IMessageStatusRegistryMySqlConfiguration configuration, IClock clock)
    {
        _configuration = configuration;
        _clock = clock;
    }

    public async Task<IMessageStatusRegistryEntry> SaveProcessing(Guid messageId)
    {
        var entry = new MessageStatusRegistryEntry(messageId, MessageStatus.Processing, _clock.UtcNow);
        const string sql = "CALL _MessageStatusMarkAsProcessing(@MessageId, @MessageStatus, @CreatedAt)";
        await ExecuteNonQuery(sql, GetParameters(entry)).ConfigureAwait(false);
        return entry;
    }

    public async Task<IMessageStatusRegistryEntry> SaveSucceeded(Guid messageId)
    {
        var entry = new MessageStatusRegistryEntry(messageId, MessageStatus.Succeeded, _clock.UtcNow);
        const string sql = "CALL _MessageStatusUpdate(@MessageId, @MessageStatus, @CreatedAt)";
        var parameters = GetParameters(entry);
        // Message processed , SaveSucceeded must be successful
        // Retry 3 time , total delay 6 seconds 
        const int retryCount = 3;
        for (byte i = 0; i < retryCount; i++)
        {
            try
            {
                await ExecuteNonQuery(sql, parameters).ConfigureAwait(false);
                break;
            }
            catch (DbException ex) when (ex.IsTransient)
            {
                if (i == retryCount - 1)
                {
                    throw;
                }

                await Task.Delay(1000 * 3).ConfigureAwait(false);
            }
        }

        return entry;
    }

    public async Task<IMessageStatusRegistryEntry> SaveFaulted(Guid messageId, MessageFaultDetails messageFaultDetails)
    {
        var entry = new MessageStatusRegistryEntry(messageId, MessageStatus.Faulted, _clock.UtcNow, messageFaultDetails);
        const string sql = "CALL _MessageStatusMarkAsFaulted(@MessageId, @MessageStatus, @CreatedAt, @FaultDetails)";
        await ExecuteNonQuery(sql, GetParameters(entry)).ConfigureAwait(false);
        return entry;
    }

    public async Task<IMessageStatusRegistryEntry?> GetLastEntry(Guid messageId)
    {
        const string sql = "SELECT * FROM _MessageStatusHistory " +
                           "WHERE MessageId=@MessageId ORDER BY Id " +
                           "DESC LIMIT 1";

        var entries = await ExecuteReader(sql, messageId).ConfigureAwait(false);
        return entries.FirstOrDefault();
    }


    public async Task<IEnumerable<IMessageStatusRegistryEntry>> GetEntries(Guid messageId)
    {
        const string sql = "SELECT * FROM _MessageStatusHistory " +
                           "WHERE MessageId=@MessageId ORDER BY Id ";

        return await ExecuteReader(sql, messageId).ConfigureAwait(false);
    }

    private async Task ExecuteNonQuery(string commandText, MySqlParameter[] parameters)
    {
        await using var connection = new MySqlConnection(_configuration.GetConnectionString());
        {
            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            command.Parameters.AddRange(parameters);
            await connection.OpenAsync().ConfigureAwait(false);
            connection.EnlistTransaction(Transaction.Current);
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    private async Task<IEnumerable<IMessageStatusRegistryEntry>> ExecuteReader(string commandText, Guid messageId)
    {
        var records = new List<object[]>();
        await using var connection = new MySqlConnection(_configuration.GetConnectionString());
        {
            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            command.Parameters.Add(new MySqlParameter("@MessageId", MySqlDbType.Binary) { Value = messageId.ToByteArray() });
            await connection.OpenAsync().ConfigureAwait(false);
            await using var dataReader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await dataReader.ReadAsync())
            {
                //DbConnection ın açık kaldığı süreyi minimize etmek için değerler array a alınıyor.
                //connection kapatıldıktan sonra json deserialization ve diğer işlemler ToEntries metodunda yapılıyor.
                var values = new object[3];
                values[0] = dataReader.GetValue("MessageStatus");
                values[1] = dataReader.GetDateTimeOffset("CreatedAt");
                values[2] = dataReader.GetValue("FaultDetails");
                records.Add(values);
            }
        }

        return ToEntries(records, messageId);
    }


    private static MySqlParameter[] GetParameters(IMessageStatusRegistryEntry entry)
    {
        List<MySqlParameter> parameters = new()
        {
            new MySqlParameter("@MessageId", MySqlDbType.Binary) { Value = entry.MessageId.ToByteArray() },
            new MySqlParameter("@MessageStatus", MySqlDbType.Int32) { Value = (int)entry.MessageStatus },
            new MySqlParameter("@CreatedAt", MySqlDbType.Newdate) { Value = entry.CreatedAt }
        };

        if (entry.MessageStatus == MessageStatus.Faulted)
        {
            object value = entry.FaultDetails == null ? DBNull.Value : JsonSerde.Serialize(entry.FaultDetails);
            parameters.Add(new MySqlParameter("@FaultDetails", MySqlDbType.JSON) { Value = value });
        }

        return parameters.ToArray();
    }

    private static IEnumerable<IMessageStatusRegistryEntry> ToEntries(IEnumerable<object[]> dataRecords, Guid messageId)
    {
        var entries = new List<IMessageStatusRegistryEntry>();
        foreach (var dataRecord in dataRecords)
        {
            var messageStatus = (MessageStatus)dataRecord[0];
            var createdAt = (DateTimeOffset)dataRecord[1];
            var faultDetailsJsonText = dataRecord[2];
            MessageFaultDetails? faultDetailsObject = null;
            if (faultDetailsJsonText != DBNull.Value)
            {
                faultDetailsObject = JsonSerde.Deserialize<MessageFaultDetails?>((string)faultDetailsJsonText);
            }

            entries.Add(new MessageStatusRegistryEntry(
                messageId,
                messageStatus,
                createdAt,
                faultDetailsObject
            ));
        }

        return entries;
    }
}