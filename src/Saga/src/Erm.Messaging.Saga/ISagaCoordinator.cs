using System;
using System.Threading.Tasks;
using Erm.Messaging;

namespace Erm.Messaging.Saga;

public interface ISagaCoordinator
{
    Task Process<TMessage>(
        IReceiveContext context,
        IEnvelope<TMessage> envelope,
        Func<IReceiveContext, IEnvelope<TMessage>, Task>? onCompleted = null,
        Func<IReceiveContext, IEnvelope<TMessage>, Task>? onRejected = null
    ) where TMessage : class;
}