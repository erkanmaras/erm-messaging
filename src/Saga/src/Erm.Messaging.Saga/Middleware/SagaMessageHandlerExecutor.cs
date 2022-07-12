using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Erm.Messaging;

namespace Erm.Messaging.Saga;

internal abstract class SagaMessageHandlerExecutor
{
    private static readonly ConcurrentDictionary<Type, SagaMessageHandlerExecutor> Executors = new();

    public static SagaMessageHandlerExecutor GetExecutor(Type messageType)
    {
        return Executors.GetOrAdd(messageType, ExecutorFactory);

        SagaMessageHandlerExecutor ExecutorFactory(Type _)
        {
            return (SagaMessageHandlerExecutor)Activator.CreateInstance(typeof(InternalExecutor<>).MakeGenericType(messageType))!;
        }
    }

    public abstract Task Execute(ISagaCoordinator handler, IReceiveContext context, object envelope);

    //TODO IMessage->TMessage a çevirmek için var.Daha performanslı bir alternatif mümkün mü?
    private class InternalExecutor<TMessage> : SagaMessageHandlerExecutor where TMessage : class
    {
        public override Task Execute(ISagaCoordinator coordinator, IReceiveContext context, object envelope)
        {
            var typedEnvelope = (IEnvelope<TMessage>)envelope;
            return coordinator.Process(context, typedEnvelope);
        }
    }
}