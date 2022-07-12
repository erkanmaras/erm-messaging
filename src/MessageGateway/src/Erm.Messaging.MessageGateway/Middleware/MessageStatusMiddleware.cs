using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Erm.Core;
using Erm.Messaging;
using Erm.Messaging.Pipeline;

namespace Erm.Messaging.MessageGateway;

public class MessageStatusMiddleware : IMessagePipelineMiddleware<IReceiveContext>
{
    private readonly IMessageStatusRegistry _messageStatusRegistry;
    private readonly IClock _clock;
    private readonly ILogger<MessageStatusMiddleware> _logger;

    public MessageStatusMiddleware(IMessageStatusRegistry messageStatusRegistry, IClock clock, ILogger<MessageStatusMiddleware> logger)
    {
        _messageStatusRegistry = messageStatusRegistry;
        _clock = clock;
        _logger = logger;
    }

    public async Task Invoke(IReceiveContext context, IEnvelope envelope, NextDelegate<IReceiveContext> next)
    {
        try
        {
            await MarkAsProcessing(envelope);

            if (IsExpired(envelope))
            {
                await MarkAsExpired(envelope);
                context.Result.SetFaulted(ReceiveFailReason.TimeToLiveExpired);
                return;
            }

            await next(context, envelope).ConfigureAwait(false);
            await MarkAsSucceeded(envelope);
        }
        catch (DuplicateMessageProcessingException)
        {
            context.Result.SetFaulted(ReceiveFailReason.Duplicate);
        }
        catch (Exception ex)
        {
            await MarkAsFaulted(envelope, ex);
            throw;
        }
    }

    private async Task MarkAsProcessing(IEnvelope envelope)
    {
        var entry = await _messageStatusRegistry.MarkAsProcessing(envelope.MessageId);
        _logger.MessageStatusChanged(entry.MessageStatus);
    }

    private async Task MarkAsSucceeded(IEnvelope envelope)
    {
        try
        {
            var entry = await _messageStatusRegistry.MarkAsSucceeded(envelope.MessageId);
            _logger.MessageStatusChanged(entry.MessageStatus);
        }
        catch (Exception ex)
        {
            _logger.MarkAsSucceedFailed(ex);
            //ignored
            //Show must go on....
        }
    }

    private async Task MarkAsFaulted(IEnvelope envelope, Exception ex)
    {
        var entry = await _messageStatusRegistry.MarkAsFaulted(envelope.MessageId, new MessageFaultDetails(ex.GetType().ToString(), ex.Message));
        _logger.MessageStatusChanged(entry.MessageStatus);
    }

    private async Task MarkAsExpired(IEnvelope envelope)
    {
        var entry = await _messageStatusRegistry.MarkAsFaulted(envelope.MessageId, new MessageFaultDetails("Skipped", "Message TTL Expired!"));
        _logger.MessageStatusChanged(entry.MessageStatus);
    }

    private bool IsExpired(IEnvelope envelope)
    {
        return envelope.IsExpired(_clock.UtcNow);
    }
}