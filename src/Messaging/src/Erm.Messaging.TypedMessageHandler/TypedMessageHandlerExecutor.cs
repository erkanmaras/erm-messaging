using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Erm.Messaging;

namespace Erm.Messaging.TypedMessageHandler;

internal abstract class TypedMessageHandlerExecutor
{
    private static readonly ConcurrentDictionary<Type, TypedMessageHandlerExecutor> Executors = new();

    public static TypedMessageHandlerExecutor GetExecutor(Type messageType)
    {
        return Executors.GetOrAdd(messageType, ExecutorFactory);

        TypedMessageHandlerExecutor ExecutorFactory(Type _)
        {
            return (TypedMessageHandlerExecutor)Activator.CreateInstance(typeof(InternalExecutor<>).MakeGenericType(messageType))!;
        }
    }

    public abstract Task Execute(object handler, IReceiveContext context, object envelope);

    private class InternalExecutor<TMessage> : TypedMessageHandlerExecutor where TMessage : class
    {
        public override Task Execute(object handler, IReceiveContext context, object envelope)
        {
            var typedHandler = (IMessageHandler<TMessage>)handler;
            var typedEnvelope = (IEnvelope<TMessage>)envelope;
            return typedHandler.Handle(context, typedEnvelope);
        }
    }
}