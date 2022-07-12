using System;
using System.Threading.Tasks;
using Erm.Messaging.Pipeline;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Erm.Core;

namespace Erm.Messaging;

[PublicAPI]
public class MessageSender : IMessageSender
{
    private readonly IMetadataProvider _metadataProvider;
    private readonly IPipelineExecutor<ISendContext> _pipelineExecutor;
    private readonly IClock _clock;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MessageSender> _logger;

    public MessageSender(
        IMetadataProvider metadataProvider,
        IPipelineExecutor<ISendContext> pipelineExecutor,
        IServiceScopeFactory serviceScopeFactory,
        IClock clock,
        ILogger<MessageSender> logger)
    {
        _metadataProvider = metadataProvider;
        _pipelineExecutor = pipelineExecutor;
        _clock = clock;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<SendResult> Send<TMessage>(Envelope<TMessage> envelope) where TMessage : class
    {
        using (_logger.BeginScope("Message {MessageName}[{MessageId}]", envelope.MessageName, envelope.MessageId))
        {
            try
            {
                _logger.MessageSendStart();
                SetDefaultProperties(envelope);
                if (string.IsNullOrWhiteSpace(envelope.ReplyTo))
                {
                    envelope.ReplyTo = _metadataProvider.GetReplyToAddress(envelope.MessageName);
                }

                SendContext context;
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    context = new SendContext(scope.ServiceProvider);
                    await _pipelineExecutor.Execute(context, envelope);
                }

                _logger.MessageSendEnd(context.Result.Succeeded);
                return context.Result;
            }
            catch (Exception e)
            {
                _logger.MessageSendFailed(e);
                throw;
            }
        }
    }

    public Task<SendResult> Send<TMessage>(TMessage message) where TMessage : class => Send(new Envelope<TMessage>(message));

    private void SetDefaultProperties(IEnvelope envelope)
    {
        if (string.IsNullOrWhiteSpace(envelope.Destination))
        {
            envelope.Destination = _metadataProvider.GetDestinationAddress(envelope.MessageName);
        }

        if (string.IsNullOrWhiteSpace(envelope.Source))
        {
            envelope.Source = _metadataProvider.MessageDomain;
        }

        envelope.Time ??= _clock.UtcNow;
    }
}