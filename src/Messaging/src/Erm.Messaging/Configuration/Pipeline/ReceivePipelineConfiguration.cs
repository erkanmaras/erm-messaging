using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Erm.Messaging.Pipeline;
using Erm.Messaging.Pipeline.Middleware;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class ReceivePipelineConfiguration
{
    private readonly List<Factory<IMessagePipelineMiddleware<IReceiveContext>>> _middlewares = new();

    // ReSharper disable once ReturnTypeCanBeEnumerable.Global
    public ReadOnlyCollection<Factory<IMessagePipelineMiddleware<IReceiveContext>>> Middlewares => _middlewares.AsReadOnly();

    public void Use(Factory<IMessagePipelineMiddleware<IReceiveContext>> middleware)
    {
        _middlewares.Add(middleware);
    }

    public void Use(IEnumerable<Factory<IMessagePipelineMiddleware<IReceiveContext>>> middlewares)
    {
        _middlewares.AddRange(middlewares);
    }

    public void Use(Func<IReceiveContext, IEnvelope, NextDelegate<IReceiveContext>, Task> middleware)
    {
        _middlewares.Add(_ => new LamdaMiddleware<IReceiveContext>(middleware));
    }
}