using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Erm.Messaging.MessageGateway;

[PublicAPI]
public interface IMessageStatusRegistry
{
    Task<IMessageStatusRegistryEntry> MarkAsProcessing(Guid messageId);

    Task<IMessageStatusRegistryEntry> MarkAsSucceeded(Guid messageId);

    Task<IMessageStatusRegistryEntry> MarkAsFaulted(Guid messageId, MessageFaultDetails faultDetails);

    Task<IMessageStatusRegistryEntry?> GetLastEntry(Guid messageId);

    Task<IEnumerable<IMessageStatusRegistryEntry>> GetEntries(Guid messageId);
}