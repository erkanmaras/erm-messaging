using System.Threading.Tasks;
using Erm.Messaging;
using Erm.Messaging.Pipeline;

namespace Erm.Messaging.Saga;

public class SagaMessageHandlerMiddleware : IMessagePipelineMiddleware<IReceiveContext>
{
    private readonly ISagaCoordinator _coordinator;

    public SagaMessageHandlerMiddleware(ISagaCoordinator coordinator)
    {
        _coordinator = coordinator;
    }

    public async Task Invoke(IReceiveContext context, IEnvelope envelope, NextDelegate<IReceiveContext> next)
    {
        var messageType = envelope.Message.GetType();
        var executor = SagaMessageHandlerExecutor.GetExecutor(messageType);
        await executor.Execute(_coordinator, context, envelope).ConfigureAwait(false);
        await next(context, envelope).ConfigureAwait(false);
    }
}