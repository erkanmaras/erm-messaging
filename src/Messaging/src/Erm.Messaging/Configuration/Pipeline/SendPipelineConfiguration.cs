using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Erm.Messaging.Pipeline;
using Erm.Messaging.Pipeline.Middleware;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging;

[PublicAPI]
public class SendPipelineConfiguration
{
    private readonly List<Factory<IMessagePipelineMiddleware<ISendContext>>> _middlewares = new();
    public IEnumerable<Factory<IMessagePipelineMiddleware<ISendContext>>> Middlewares => _middlewares;

    public void Use(Factory<IMessagePipelineMiddleware<ISendContext>> middleware)
    {
        _middlewares.Add(middleware);
    }

    public void Use(IEnumerable<Factory<IMessagePipelineMiddleware<ISendContext>>> middlewares)
    {
        _middlewares.AddRange(middlewares);
    }

    public void Use(Func<ISendContext, IEnvelope, NextDelegate<ISendContext>, Task> middleware)
    {
        _middlewares.Add(_ => new LamdaMiddleware<ISendContext>(middleware));
    }

    public void UseSendTransport()
    {
        Use(provider => new SendEndpointMiddleware(provider.GetRequiredService<ISendEndpoint>()));
    }
}