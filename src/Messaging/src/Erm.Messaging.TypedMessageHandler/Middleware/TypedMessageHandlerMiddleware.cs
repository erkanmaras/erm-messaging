using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Erm.Messaging;
using Erm.Messaging.Pipeline;

namespace Erm.Messaging.TypedMessageHandler.Middleware;

public class TypedMessageHandlerMiddleware : IMessagePipelineMiddleware<IReceiveContext>
{
    private readonly IMessageHandlerTypeMap _typeMap;
    private readonly ILogger<TypedMessageHandlerMiddleware> _logger;

    public TypedMessageHandlerMiddleware(IMessageHandlerTypeMap typeMap, ILogger<TypedMessageHandlerMiddleware> logger)
    {
        _typeMap = typeMap;
        _logger = logger;
    }

    public async Task Invoke(IReceiveContext context, IEnvelope envelope, NextDelegate<IReceiveContext> next)
    {
        var handlersTypes = _typeMap.GetMessageHandlerTypes(envelope.Message.GetType()).ToList();
        if (handlersTypes.Count > 0)
        {
            var tasks = new List<Task>(handlersTypes.Count);
            for (var i = 0; i < handlersTypes.Count; i++)
            {
                var handlerType = handlersTypes[i];
                tasks.Add(Task.Run(async () =>
                {
                    var messageType = envelope.Message.GetType();
                    var executor = TypedMessageHandlerExecutor.GetExecutor(messageType);

                    await executor.Execute(context.ServiceProvider.GetRequiredService(handlerType), context, envelope);
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        else
        {
            _logger.NoMessageHandlerFound();
        }

        await next(context, envelope);
    }
}