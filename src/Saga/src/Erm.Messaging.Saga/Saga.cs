using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Erm.Messaging;

namespace Erm.Messaging.Saga;

[PublicAPI]
public abstract class Saga : ISaga
{
    private object? _data;
    public Guid SagaId { get; private set; }
    public SagaStatus Status { get; private set; }

    public virtual void Initialize(Guid sagaId, SagaStatus status, object? data = null)
    {
        SagaId = sagaId;
        Status = status;
        _data = data;
    }

    public Guid GetSagaId<TMessage>(IReceiveContext context, IEnvelope<TMessage> envelope, bool isStartAction) where TMessage : class
    {
        var id = ResolveId(context, envelope);

        if (id == Guid.Empty)
        {
            // TODO : Should we throw a well defined exception instead of SagaException?
            return isStartAction ? envelope.MessageId : throw new SagaException("SagaId cannot be resolved!");
        }

        return id;
    }

    protected virtual Guid ResolveId<TMessage>(IReceiveContext context, IEnvelope<TMessage> envelope) where TMessage : class
    {
        return envelope.CorrelationId ?? default;
    }

    public virtual Task Complete()
    {
        Status = SagaStatus.Completed;
        return Task.CompletedTask;
    }

    public virtual void Reject(Exception ex)
    {
        Status = SagaStatus.Rejected;
    }

    public object? GetData()
    {
        return _data;
    }
}