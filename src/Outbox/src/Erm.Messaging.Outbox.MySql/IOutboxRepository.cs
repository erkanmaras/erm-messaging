using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Erm.MessageOutbox;

namespace Erm.Messaging.Outbox.MySql;

[PublicAPI]
public interface IOutboxRepository
{
    Task Save(IMessageOutboxEntry entry);
    Task<MySqlMessageOutboxEntry?> GetById(Guid entryId);
}