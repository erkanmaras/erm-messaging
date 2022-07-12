using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using Erm.Core;
using Erm.Messaging.MessageGateway;

namespace Erm.Messaging.MessageGateway.MySql;

public class MySqlMessageStatusRegistry : IMessageStatusRegistry
{
    private readonly MessageStatusRegistryRepository _repository;

    public MySqlMessageStatusRegistry(
        IMessageStatusRegistryMySqlConfiguration configuration,
        IClock clock)
    {
        _repository = new MessageStatusRegistryRepository(configuration, clock);
    }

    public async Task<IMessageStatusRegistryEntry> MarkAsProcessing(Guid messageId)
    {
        try
        {
            var entry = await _repository.SaveProcessing(messageId);
            return entry;
        } //if messageId exist in db
        catch (MySqlException ex) when (ex.Number == (int)MySqlErrorCode.DuplicateKeyEntry)
        {
            throw new DuplicateMessageProcessingException(messageId);
        }
    }

    public Task<IMessageStatusRegistryEntry> MarkAsSucceeded(Guid messageId)
    {
        return _repository.SaveSucceeded(messageId);
    }

    public async Task<IMessageStatusRegistryEntry> MarkAsFaulted(Guid messageId, MessageFaultDetails messageFaultDetails)
    {
        return await _repository.SaveFaulted(messageId, messageFaultDetails);
    }

    public Task<IMessageStatusRegistryEntry?> GetLastEntry(Guid messageId)
    {
        return _repository.GetLastEntry(messageId);
    }

    public Task<IEnumerable<IMessageStatusRegistryEntry>> GetEntries(Guid messageId)
    {
        return _repository.GetEntries(messageId);
    }
}