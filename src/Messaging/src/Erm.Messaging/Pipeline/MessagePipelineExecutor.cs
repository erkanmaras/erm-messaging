using System.Threading.Tasks;

namespace Erm.Messaging.Pipeline;

internal class MessagePipelineExecutor<TContext> : IPipelineExecutor<TContext> where TContext : IMessageContext
{
    private readonly IMessagePipeline<TContext> _pipeline;

    public MessagePipelineExecutor(IMessagePipeline<TContext> pipeline)
    {
        _pipeline = pipeline;
    }

    public Task Execute(TContext context, IEnvelope envelope)
    {
        return _pipeline.Invoke(context, envelope);
    }
}