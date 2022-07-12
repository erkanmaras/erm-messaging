using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using MySqlConnector;
using Erm.Messaging;
using Erm.Messaging.Saga;
using Erm.Serialization.Json;

namespace Erm.Messaging.Saga.MySql;

public class MySqlSagaRepository : ISagaRepository
{
    private readonly IMySqlSagaConfiguration _configuration;
    private readonly IMetadataProvider _metadataProvider;

    public MySqlSagaRepository(IMySqlSagaConfiguration configuration, IMetadataProvider metadataProvider)
    {
        _configuration = configuration;
        _metadataProvider = metadataProvider;
    }

    public async Task SaveActionLog(ISagaActionLogEntry logEntry)
    {
        await using var connection = new MySqlConnection(_configuration.GetConnectionString());
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO _SagaActionLog(SagaId, MessageName, Envelope, CreatedAt) " +
                                  "VALUES (@SagaId, @MessageName, @Envelope, @CreatedAt)";

            command.Parameters.Add(new MySqlParameter("@SagaId", MySqlDbType.Binary) { Value = logEntry.SagaId });
            command.Parameters.Add(new MySqlParameter("@MessageName", MySqlDbType.VarChar) { Value = logEntry.MessageName });
            command.Parameters.Add(new MySqlParameter("@Envelope", MySqlDbType.JSON) { Value = JsonSerde.Serialize(logEntry.Envelope, logEntry.Envelope.GetType()) });
            command.Parameters.Add(new MySqlParameter("@CreatedAt", MySqlDbType.Newdate) { Value = logEntry.CreatedAt });
            await connection.OpenAsync().ConfigureAwait(false);
            connection.EnlistTransaction(Transaction.Current);
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<ISagaActionLogEntry>> GetActionLogs(Guid sagaId)
    {
        var valueList = new List<object[]>();
        await using var connection = new MySqlConnection(_configuration.GetConnectionString());
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM _SagaActionLog " +
                                  "WHERE SagaId=@SagaId";

            command.Parameters.Add(new MySqlParameter("@SagaId", MySqlDbType.Binary) { Value = sagaId });
            await connection.OpenAsync();
            await using var dataReader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await dataReader.ReadAsync())
            {
                return Enumerable.Empty<ISagaActionLogEntry>();
            }

            var values = new object[3];
            values[0] = dataReader.GetString("MessageName");
            values[1] = dataReader.GetString("Envelope");
            values[2] = dataReader.GetDateTimeOffset("CreatedAt");
            valueList.Add(values);
        }
        return ToActionLogEntries(sagaId, valueList);
    }


    public async Task<ISagaStateEntry?> GetState(Guid sagaId, Type sagaType)
    {
        var values = new object[4];
        await using var connection = new MySqlConnection(_configuration.GetConnectionString());
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM _SagaState " +
                                  "WHERE SagaId=@SagaId AND SagaType=@SagaType";

            command.Parameters.Add(new MySqlParameter("@SagaId", MySqlDbType.Binary) { Value = sagaId });
            command.Parameters.Add(new MySqlParameter("@SagaType", MySqlDbType.VarChar) { Value = sagaType.FullName });
            await connection.OpenAsync();
            await using var dataReader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await dataReader.ReadAsync())
            {
                return null;
            }

            values[0] = dataReader.GetValue("Id");
            values[1] = dataReader.GetValue("SagaStatus");
            values[2] = dataReader.GetValue("Data");
            values[3] = dataReader.GetValue("RowVersion");
        }
        return ToStateEntry(sagaId, sagaType, values);
    }

    public Task SaveState(ISagaStateEntry stateEntry)
    {
        var mySqlSagaStateEntry = (MySqlSagaStateEntry)stateEntry;
        return mySqlSagaStateEntry.Id.HasValue ? UpdateState(mySqlSagaStateEntry) : InsertState(mySqlSagaStateEntry);
    }

    private async Task InsertState(ISagaStateEntry stateEntry)
    {
        await using var connection = new MySqlConnection(_configuration.GetConnectionString());
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO _SagaState(SagaId, SagaType, SagaStatus, Data, RowVersion) " +
                                  "VALUES (@SagaId, @SagaType, @SagaStatus, @Data, @RowVersion) " +
                                  "ON DUPLICATE KEY UPDATE SagaStatus=@SagaStatus, Data=@Data, RowVersion=RowVersion + 1";

            command.Parameters.Add(new MySqlParameter("@SagaId", MySqlDbType.Binary) { Value = stateEntry.SagaId });
            command.Parameters.Add(new MySqlParameter("@SagaType", MySqlDbType.VarChar) { Value = stateEntry.SagaType.FullName });
            command.Parameters.Add(new MySqlParameter("@SagaStatus", MySqlDbType.Int32) { Value = (int)stateEntry.Status });
            command.Parameters.Add(new MySqlParameter("@Data", MySqlDbType.JSON) { Value = ToJson(stateEntry.Data) });
            command.Parameters.Add(new MySqlParameter("@RowVersion", MySqlDbType.Int32) { Value = 1 });
            await connection.OpenAsync();
            connection.EnlistTransaction(Transaction.Current);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task UpdateState(MySqlSagaStateEntry stateEntry)
    {
        await using var connection = new MySqlConnection(_configuration.GetConnectionString());
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE _SagaState " +
                                  "SET SagaId=@SagaId, SagaType=@SagaType, SagaStatus=@SagaStatus, Data=@Data, RowVersion=RowVersion + 1 " +
                                  "WHERE Id=@Id AND RowVersion=@RowVersion";

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.UInt64) { Value = stateEntry.Id!.Value });
            command.Parameters.Add(new MySqlParameter("@SagaId", MySqlDbType.Binary) { Value = stateEntry.SagaId });
            command.Parameters.Add(new MySqlParameter("@SagaType", MySqlDbType.VarChar) { Value = stateEntry.SagaType.FullName });
            command.Parameters.Add(new MySqlParameter("@SagaStatus", MySqlDbType.Int32) { Value = (int)stateEntry.Status });
            command.Parameters.Add(new MySqlParameter("@Data", MySqlDbType.JSON) { Value = ToJson(stateEntry.Data) });
            command.Parameters.Add(new MySqlParameter("@RowVersion", MySqlDbType.Int32) { Value = stateEntry.RowVersion });
            await connection.OpenAsync();
            connection.EnlistTransaction(Transaction.Current);
            var effectedRow = await command.ExecuteNonQueryAsync();
            //Optimistic Concurrency
            if (effectedRow == 0)
            {
                throw new SagaStateConcurrencyException($"Update saga state failed! SagaId:{stateEntry.SagaId} SagaType:{stateEntry.SagaType.FullName}");
            }
        }
    }

    public ISagaStateEntry CreateStateEntry(Guid sagaId, Type sagaType, SagaStatus status, object? data = null)
    {
        return new MySqlSagaStateEntry(sagaId, sagaType, status, data, rowVersion: 1);
    }

    public ISagaActionLogEntry CreateActionLogEntry(Guid sagaId, DateTimeOffset createdAt, IEnvelope envelope)
    {
        return new MySqlSagaActionLogEntry(sagaId, createdAt, envelope);
    }

    private IEnumerable<ISagaActionLogEntry> ToActionLogEntries(Guid sagaId, IEnumerable<object[]> valueList)
    {
        var entries = new List<MySqlSagaActionLogEntry>();
        foreach (var values in valueList)
        {
            var messageTypeName = (string)values[0];
            var messageJsonText = (string)values[1];
            var createdAt = (DateTimeOffset)values[2];
            var messageType = ResolveMessageType(messageTypeName);
            var openEnvelopeType = typeof(Envelope<>);
            var closedEnvelopeType = openEnvelopeType.MakeGenericType(messageType);

            var envelope = FromJson(messageJsonText, closedEnvelopeType);
            if (envelope == null)
            {
                throw new Exception("SagaActionLog envelope can't serialize!");
            }

            entries.Add(new MySqlSagaActionLogEntry(sagaId, createdAt, (IEnvelope)envelope));
        }

        return entries;
    }

    private static ISagaStateEntry ToStateEntry(Guid sagaId, Type sagaType, IReadOnlyList<object> values)
    {
        var id = (ulong)values[0];
        var sagaStatus = (SagaStatus)values[1];
        var dataJsonText = values[2];
        var rowVersion = (int)values[3];

        if (!sagaType.BaseType!.IsGenericType || dataJsonText == DBNull.Value)
        {
            return new MySqlSagaStateEntry(id, sagaId, sagaType, sagaStatus, data: null, rowVersion);
        }

        var dataType = sagaType.BaseType.GenericTypeArguments.First();
        var data = FromJson((string)dataJsonText, dataType);
        return new MySqlSagaStateEntry(id, sagaId, sagaType, sagaStatus, data, rowVersion);
    }

    private Type ResolveMessageType(string messageName)
    {
        var messageType = _metadataProvider.GetMessageObjectType(messageName);
        if (messageType is null)
        {
            throw new Exception($"Received message-name:{messageName} not found in message-type-map!");
        }

        return messageType;
    }

    private static string? ToJson(object? data)
    {
        return data == null ? null : JsonSerde.Serialize(data);
    }

    private static object? FromJson(string json, Type type)
    {
        return JsonSerde.Deserialize(json, type);
    }
}