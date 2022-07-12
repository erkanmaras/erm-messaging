using System;
using System.Threading.Tasks;

namespace Erm.Messaging.Pipeline.Middleware;

public class LamdaMiddleware<TMessageContext> : IMessagePipelineMiddleware<TMessageContext> where TMessageContext : IMessageContext
{
    private readonly Func<TMessageContext, IEnvelope, NextDelegate<TMessageContext>, Task> _func;

    public LamdaMiddleware(Func<TMessageContext, IEnvelope, NextDelegate<TMessageContext>, Task> func)
    {
        _func = func;
    }

    public Task Invoke(TMessageContext context, IEnvelope envelope, NextDelegate<TMessageContext> next)
    {
        return _func(context, envelope, next);
    }
}