using System;
using System.Threading.Tasks;
using Erm.Messaging;

namespace Erm.Messaging.Saga;

internal interface ISagaProcessor
{
    Task Process<TMessage>(
        ISaga saga,
        IReceiveContext context,
        IEnvelope<TMessage> envelope,
        ISagaStateEntry stateEntry
    ) where TMessage : class;

    Task PostProcess<TMessage>(
        ISaga saga,
        IReceiveContext context,
        IEnvelope<TMessage> envelope,
        Func<IReceiveContext, IEnvelope<TMessage>, Task>? onCompleted,
        Func<IReceiveContext, IEnvelope<TMessage>, Task>? onRejected) where TMessage : class;
}