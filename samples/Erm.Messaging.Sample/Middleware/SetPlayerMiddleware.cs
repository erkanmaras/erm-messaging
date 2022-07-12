using Erm.Messaging.Pipeline;

namespace Erm.Messaging.Sample;

public class SetPlayerMiddleware<TContext> : IMessagePipelineMiddleware<TContext> where TContext : IMessageContext
{
    public Task Invoke(TContext context, IEnvelope envelope, NextDelegate<TContext> next)
    {
        envelope.ExtendedProperties["Player"] = "Anonymous";
        return next(context, envelope);
    }
}