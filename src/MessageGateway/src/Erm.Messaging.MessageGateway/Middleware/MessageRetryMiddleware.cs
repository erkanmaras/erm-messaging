using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Erm.Messaging;
using Erm.Messaging.Pipeline;

namespace Erm.Messaging.MessageGateway;

public class MessageRetryMiddleware : IMessagePipelineMiddleware<IReceiveContext>
{
    private readonly MessageRetryConfiguration _retryConfiguration;
    private readonly ILogger<MessageRetryMiddleware> _logger;

    public MessageRetryMiddleware(MessageRetryConfiguration retryConfiguration, ILogger<MessageRetryMiddleware> logger)
    {
        _retryConfiguration = retryConfiguration;
        _logger = logger;
    }

    public async Task Invoke(IReceiveContext context, IEnvelope envelope, NextDelegate<IReceiveContext> next)
    {
        var exceptions = new List<Exception>(_retryConfiguration.RetryCount);
        for (byte i = 0; _retryConfiguration.RetryCount == 0 || i <= _retryConfiguration.RetryCount; i++)
        {
            try
            {
                await next(context, envelope).ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                if (_retryConfiguration.RetryCount > 0 && (_retryConfiguration.Policy?.Invoke(ex, envelope, i) ?? true))
                {
                    exceptions.Add(ex);
                    if (i >= _retryConfiguration.RetryCount)
                    {
                        throw new AggregateException(exceptions);
                    }

                    _logger.ErrorOccurredRetrying(ex, i);
                    await Task.Delay((i + 1) * 1000 * _retryConfiguration.BackoffDuration);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}