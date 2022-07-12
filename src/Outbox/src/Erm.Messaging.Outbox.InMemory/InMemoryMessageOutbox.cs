using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Transactions;
using Erm.Core;
using Erm.MessageOutbox;
using Erm.Messaging;

namespace Erm.Messaging.Outbox.InMemory;

internal class InMemoryMessageOutbox : IMessageOutbox
{
    private readonly IClock _clock;

    private ConcurrentDictionary<Guid, TransactionalEntry> OutboxMessages { get; } = new();

    public InMemoryMessageOutbox(IClock clock)
    {
        _clock = clock;
    }

    public Task<IMessageOutboxEntry?> GetEntry(Guid entryId)
    {
        OutboxMessages.TryGetValue(entryId, out var entry);
        return Task.FromResult<IMessageOutboxEntry?>(entry?.InternalEntry);
    }

    public Task Save(IMessageOutboxEntry outboxEntry)
    {
        CheckEntry(outboxEntry);

        if (OutboxMessages.TryGetValue(outboxEntry.Id, out var existingEntry))
        {
            if (outboxEntry.Equals(existingEntry.InternalEntry))
            {
                return Task.CompletedTask;
            }

            throw new InvalidOperationException("An entry with the same entryId is already added!");
        }

        var anActiveTransactionExists = Transaction.Current is not null && Transaction.Current.TransactionInformation.Status == TransactionStatus.Active;
        var inMemoryMessageOutboxEntry = (InMemoryMessageOutboxEntry)outboxEntry;
        inMemoryMessageOutboxEntry.CreatedAt = _clock.UtcNow;

        var transactionalEntry = new TransactionalEntry(inMemoryMessageOutboxEntry, !anActiveTransactionExists);
        OutboxMessages[outboxEntry.Id] = transactionalEntry;

        if (anActiveTransactionExists)
        {
            Transaction.Current?.EnlistVolatile(new EnlistmentNotification(
                    rollback: () => { OutboxMessages.TryRemove(outboxEntry.Id, out _); },
                    commit: () => { transactionalEntry.Commit(); })
                , EnlistmentOptions.None);
        }

        return Task.CompletedTask;
    }

    public IMessageOutboxEntry CreateEntry(IMessageEnvelope envelope)
    {
        return new InMemoryMessageOutboxEntry(Uuid.Next(), envelope);
    }

    private static void CheckEntry(IMessageOutboxEntry outboxEntry)
    {
        if (outboxEntry == null)
        {
            throw new ArgumentNullException(nameof(outboxEntry));
        }

        if (outboxEntry is not InMemoryMessageOutboxEntry)
        {
            throw new InvalidOperationException($"{nameof(InMemoryMessageOutboxEntry)} type was expected!");
        }

        if (outboxEntry is not InMemoryMessageOutboxEntry)
        {
            throw new InvalidOperationException($"{nameof(InMemoryMessageOutboxEntry)} type was expected!");
        }

        if (outboxEntry.Message == null)
        {
            throw new InvalidOperationException($"{nameof(outboxEntry.Message)} is empty!");
        }
    }

    private class TransactionalEntry
    {
        public InMemoryMessageOutboxEntry InternalEntry { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private bool Commited { get; set; }

        public TransactionalEntry(InMemoryMessageOutboxEntry internalEntry, bool commited = false)
        {
            InternalEntry = internalEntry;
            Commited = commited;
        }

        public void Commit() => Commited = true;
    }

    private class EnlistmentNotification : IEnlistmentNotification
    {
        private readonly Action _rollback;
        private readonly Action _commit;

        public EnlistmentNotification(Action rollback, Action commit)
        {
            _rollback = rollback;
            _commit = commit;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            _commit();
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            // TODO: Should we rollback?
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            _rollback();
            enlistment.Done();
        }
    }
}