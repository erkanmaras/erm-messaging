using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class ReceiveContext : IReceiveContext
{
    public ReceiveContext(IMessageEnvelope envelope, IMessageSender messageSender, IServiceProvider serviceProvider)
    {
        _messageSender = messageSender;
        MessageEnvelope = envelope;
        ServiceProvider = serviceProvider;
    }

    private readonly IMessageSender _messageSender;
    public IMessageEnvelope MessageEnvelope { get; }
    public ReceiveResult Result { get; } = new();
    public ContextProperties ExtendedProperties { get; } = new();
    public IServiceProvider ServiceProvider { get; }

    public Task<SendResult> Send<TMessage>(Envelope<TMessage> envelope) where TMessage : class
    {
        envelope.CorrelationId = MessageEnvelope.CorrelationId;
        return _messageSender.Send(envelope);
    }

    public Task<SendResult> Send<TMessage>(TMessage message) where TMessage : class
    {
        return Send(new Envelope<TMessage>(message));
    }

    public Task<SendResult> Respond<TMessage>(Envelope<TMessage> envelope) where TMessage : class
    {
        if (string.IsNullOrWhiteSpace(MessageEnvelope.ReplyTo))
        {
            throw new InvalidOperationException("Received envelope does not have a reply-to address!");
        }

        envelope.CorrelationId = MessageEnvelope.CorrelationId;
        envelope.Destination = MessageEnvelope.ReplyTo;
        envelope.RequestId = MessageEnvelope.MessageId;
        return _messageSender.Send(envelope);
    }

    public Task<SendResult> Respond<TMessage>(TMessage message) where TMessage : class
    {
        return Respond(new Envelope<TMessage>(message));
    }
}