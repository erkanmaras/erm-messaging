using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Erm.Messaging;

namespace Erm.Messaging.Saga;

[PublicAPI]
public interface ISaga
{
    Guid SagaId { get; }
    SagaStatus Status { get; }
    Task Complete();
    void Reject(Exception ex);
    void Initialize(Guid sagaId, SagaStatus status, object? data = null);
    Guid GetSagaId<TMessage>(IReceiveContext context, IEnvelope<TMessage> envelope, bool isStartAction) where TMessage : class;
}

[PublicAPI]
public interface ISaga<out TData> : ISaga where TData : class
{
    TData Data { get; }
}