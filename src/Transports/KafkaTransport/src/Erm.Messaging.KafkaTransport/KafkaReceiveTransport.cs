using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Erm.KafkaClient;
using Erm.KafkaClient.Consumers;

namespace Erm.Messaging.KafkaTransport;

[PublicAPI]
internal class KafkaReceiveTransport : IReceiveTransport, IConsumerMessageHandler
{
    private readonly IReceiveEndpoint _receiveEndpoint;
    private readonly IMetadataProvider _metadataProvider;
    private readonly ILogger<KafkaReceiveTransport> _logger;

    public KafkaReceiveTransport(IReceiveEndpoint receiveEndpoint, IMetadataProvider metadataProvider, ILogger<KafkaReceiveTransport> logger)
    {
        _receiveEndpoint = receiveEndpoint;
        _metadataProvider = metadataProvider;
        _logger = logger;
    }

    public async Task Handle(IKafkaMessageContext context)
    {
        var messageName = context.Headers.GetString(nameof(IMessageEnvelope.MessageName));
        if (_metadataProvider.GetMessageObjectType(messageName) is null)
        {
            // optimization
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.MessageSkipped(messageName);
            }

            return;
        }

        var messageId = context.Headers.GetString(nameof(IMessageEnvelope.MessageId));
        using (_logger.BeginMessageReceiveScope(messageName, messageId))
        {
            try
            {
                var envelope = new KafkaMessage(
                    context.ConsumerContext.Topic,
                    context.KafkaMessage.Key,
                    context.KafkaMessage.Value,
                    context.Headers).ToEnvelope();

                _logger.MessageReceiveStart();
                var receiveResult = await _receiveEndpoint.Receive(envelope).ConfigureAwait(false);

                //Offset strategy throttled
                //auto-commitOffset = false;
// The throttled strategy; kafka-client tracks each received message and monitors their acknowledgment.
// When the kafka-client finds out that all messages before a position are processed successfully,
// it commits that position. This commit happens periodically(2sec) to avoid committing too often.
// There is one detail to mention. If an old message is neither acked nor nacked, 
// the strategy cannot commit the position anymore. It will enqueue messages forever,
// waiting for that missing ack to happen. It can lead to out of memory, as the client
// would never be able to commit a position and to clear the queue.
//
// TODO kafka-client should detects this situation and reports a failure, marking the application unhealthy 

                if (receiveResult.Succeeded)
                {
                    context.ConsumerContext.StoreOffset();
                }

                _logger.MessageReceiveEnd(receiveResult.Succeeded, receiveResult.FailReason.ToString());
            }
            catch (Exception e)
            {
                _logger.MessageReceiveFailed(e);
                throw; // Kafka client logs error and continue to consume
            }
        }
    }
}