using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Erm.Messaging;

namespace Erm.MessageOutbox;

[PublicAPI]
public interface IMessageOutbox
{
    IMessageOutboxEntry CreateEntry(IMessageEnvelope envelope);
    Task<IMessageOutboxEntry?> GetEntry(Guid entryId);
    Task Save(IMessageOutboxEntry outboxEntry);

    async Task<IMessageOutboxEntry> Save(IMessageEnvelope envelope)
    {
        var entry = CreateEntry(envelope);
        await Save(entry).ConfigureAwait(false);
        return entry;
    }
}