using System;
using System.Threading.Tasks;
using Erm.Core;
using Erm.MessageOutbox;
using Erm.Messaging;

namespace Erm.Messaging.Outbox.MySql;

internal class MySqlMessageOutbox : IMessageOutbox
{
    private readonly IOutboxRepository _outboxRepository;


    public MySqlMessageOutbox(IOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    public async Task<IMessageOutboxEntry?> GetEntry(Guid entryId)
    {
        return await _outboxRepository.GetById(entryId).ConfigureAwait(false);
    }

    public async Task Save(IMessageOutboxEntry entry)
    {
        CheckEntry(entry);
        await _outboxRepository.Save(entry).ConfigureAwait(false);
    }

    public IMessageOutboxEntry CreateEntry(IMessageEnvelope envelope)
    {
        return new MySqlMessageOutboxEntry(Uuid.Next(), envelope);
    }

    private static void CheckEntry(IMessageOutboxEntry outboxEntry)
    {
        if (outboxEntry == null)
        {
            throw new ArgumentNullException(nameof(outboxEntry));
        }

        if (outboxEntry is not MySqlMessageOutboxEntry)
        {
            throw new ArgumentException($"{nameof(MySqlMessageOutboxEntry)} type was expected!");
        }

        if (outboxEntry.MessageId == Guid.Empty)
        {
            throw new ArgumentException($"{nameof(outboxEntry.MessageId)} is null!");
        }
    }
}