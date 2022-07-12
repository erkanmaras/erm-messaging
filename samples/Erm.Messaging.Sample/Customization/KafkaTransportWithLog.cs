using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Erm.KafkaClient.Producers;
using Erm.Core;
using Erm.Messaging.KafkaTransport;
using Erm.Serialization.Json;

namespace Erm.Messaging.Sample.Customization;

//Custom transport example
[PublicAPI]
public class KafkaTransportWithLog : ISendTransport
{
    private readonly IProducerAccessor _producerAccessor;
    private readonly IClock _clock;
    private readonly ILogger<KafkaTransportWithLog> _logger;

    public KafkaTransportWithLog(IProducerAccessor producerAccessor, IClock clock, ILogger<KafkaTransportWithLog> logger)
    {
        _producerAccessor = producerAccessor;
        _clock = clock;
        _logger = logger;
    }

    public async Task Send(ISendContext context, IMessageEnvelope envelope)
    {
        _logger.LogDebug("KafkaTransportLog {Envelope}", JsonSerde.Serialize(envelope));
        var kafkaTransport = new KafkaSendTransport(_producerAccessor);
        await kafkaTransport.Send(context, envelope);
    }
}