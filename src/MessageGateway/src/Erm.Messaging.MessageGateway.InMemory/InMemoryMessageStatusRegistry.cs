using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Erm.Core;
using Erm.Messaging.MessageGateway;

namespace Erm.Messaging.MessageGateway.InMemory;

public class InMemoryMessageStatusRegistry : IMessageStatusRegistry
{
    public InMemoryMessageStatusRegistry(IClock clock)
    {
        _clock = clock;
    }

    private readonly IClock _clock;
    private ConcurrentDictionary<Guid, IMessageStatusRegistryEntry> MessageStatuses { get; } = new();
    private ConcurrentDictionary<Guid, List<IMessageStatusRegistryEntry>> MessageStatusHistory { get; } = new();

    private readonly object _registryLock = new();

    public Task<IMessageStatusRegistryEntry> MarkAsProcessing(Guid messageId)
    {
        lock (_registryLock)
        {
            if (MessageStatuses.TryGetValue(messageId, out var lastStatus))
            {
                if (lastStatus.MessageStatus is MessageStatus.Processing or MessageStatus.Succeeded)
                {
                    throw new DuplicateMessageProcessingException(messageId);
                }
            }

            IMessageStatusRegistryEntry newStatus = new InMemoryMessageStatusRegistryEntry(messageId, MessageStatus.Processing, _clock.UtcNow);
            TransactionEnlist(lastStatus, newStatus);
            MessageStatuses[messageId] = newStatus;
            var statusHistory = MessageStatusHistory.GetOrAdd(messageId, _ => new List<IMessageStatusRegistryEntry>());
            statusHistory.Add(newStatus);
            return Task.FromResult(newStatus);
        }
    }

    public Task<IMessageStatusRegistryEntry> MarkAsSucceeded(Guid messageId)
    {
        lock (_registryLock)
        {
            MessageStatuses.TryGetValue(messageId, out var lastStatus);
            IMessageStatusRegistryEntry newStatus = new InMemoryMessageStatusRegistryEntry(messageId, MessageStatus.Succeeded, _clock.UtcNow);
            TransactionEnlist(lastStatus, newStatus);
            MessageStatuses[messageId] = newStatus;
            var statusHistory = MessageStatusHistory.GetOrAdd(messageId, _ => new List<IMessageStatusRegistryEntry>());
            statusHistory.Add(newStatus);
            return Task.FromResult(newStatus);
        }
    }

    public Task<IMessageStatusRegistryEntry> MarkAsFaulted(Guid messageId, MessageFaultDetails messageFaultDetails)
    {
        lock (_registryLock)
        {
            MessageStatuses.TryGetValue(messageId, out var lastStatus);
            IMessageStatusRegistryEntry newStatus = new InMemoryMessageStatusRegistryEntry(messageId, MessageStatus.Faulted, _clock.UtcNow, messageFaultDetails);
            TransactionEnlist(lastStatus, newStatus);
            MessageStatuses[messageId] = newStatus;
            var statusHistory = MessageStatusHistory.GetOrAdd(messageId, _ => new List<IMessageStatusRegistryEntry>());
            statusHistory.Add(newStatus);
            return Task.FromResult(newStatus);
        }
    }

    public Task<IMessageStatusRegistryEntry?> GetLastEntry(Guid messageId)
    {
        IMessageStatusRegistryEntry? entry;
        lock (_registryLock)
        {
            MessageStatuses.TryGetValue(messageId, out entry);
        }

        return Task.FromResult(entry);
    }

    public Task<IEnumerable<IMessageStatusRegistryEntry>> GetEntries(Guid messageId)
    {
        List<IMessageStatusRegistryEntry>? statuses;
        lock (_registryLock)
        {
            MessageStatusHistory.TryGetValue(messageId, out statuses);
        }

        return Task.FromResult(statuses ?? Enumerable.Empty<IMessageStatusRegistryEntry>());
    }

    // Poor man's transaction support
    // push old statuses to stack and restore on rollback!"
    private void TransactionEnlist(IMessageStatusRegistryEntry? lastStatus, IMessageStatusRegistryEntry newStatus)
    {
        if (newStatus == null) throw new ArgumentNullException(nameof(newStatus));
        if (Transaction.Current is not null && Transaction.Current.TransactionInformation.Status == TransactionStatus.Active)
        {
            Transaction.Current.EnlistVolatile(
                new EnlistmentNotification(
                    lastStatus,
                    newStatus,
                    (rollbackLastStatus, rollbackNewStatus) =>
                    {
                        lock (_registryLock)
                        {
                            if (rollbackLastStatus == null)
                            {
                                MessageStatuses.TryRemove(rollbackNewStatus.MessageId, out _);
                            }
                            else
                            {
                                MessageStatuses[rollbackNewStatus.MessageId] = rollbackLastStatus;
                            }

                            MessageStatusHistory[rollbackNewStatus.MessageId].Remove(rollbackNewStatus);
                        }
                    }), EnlistmentOptions.None);
        }
    }

    private class EnlistmentNotification : IEnlistmentNotification
    {
        private readonly Action<IMessageStatusRegistryEntry?, IMessageStatusRegistryEntry> _onRollback;
        private static readonly ConcurrentStack<IMessageStatusRegistryEntry?> LastStatuses = new();
        private static readonly ConcurrentStack<IMessageStatusRegistryEntry> NewStatuses = new();

        public EnlistmentNotification(IMessageStatusRegistryEntry? oldStatus, IMessageStatusRegistryEntry newStatus, Action<IMessageStatusRegistryEntry?, IMessageStatusRegistryEntry> onRollback)
        {
            LastStatuses.Push(oldStatus);
            NewStatuses.Push(newStatus);
            _onRollback = onRollback;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            LastStatuses.TryPop(out var lastStatus);
            NewStatuses.TryPop(out var newStatus);
            _onRollback(lastStatus, newStatus!);

            enlistment.Done();
        }
    }
}