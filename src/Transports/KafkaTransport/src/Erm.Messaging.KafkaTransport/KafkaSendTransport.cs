using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Erm.KafkaClient.Producers;

namespace Erm.Messaging.KafkaTransport;

public class KafkaSendTransport : ISendTransport
{
    private const string KafkaPrefix = "_Kafka_";
    private readonly IProducerAccessor _producerAccessor;

    public KafkaSendTransport(IProducerAccessor producerAccessor)
    {
        _producerAccessor = producerAccessor;
    }

    public async Task Send(ISendContext context, IMessageEnvelope envelope)
    {
        const string producerName = nameof(ISendTransport);
        var producer = _producerAccessor[producerName];
        if (producer == null)
        {
            throw new InvalidOperationException($"Unable to find MessageProducer with name: {producerName}.");
        }

        var kafkaMessage = KafkaMessage.FromEnvelope(envelope);
        var produceResult = await producer.ProduceAsync(
            kafkaMessage.Topic,
            kafkaMessage.MessageKey,
            kafkaMessage.MessageValue,
            kafkaMessage.Headers).ConfigureAwait(false);

        context.Result.Succeeded = produceResult.Status is PersistenceStatus.Persisted or PersistenceStatus.PossiblyPersisted;
        context.ExtendedProperties.Set(KafkaPrefix + nameof(produceResult.Status), produceResult.Status);
        context.ExtendedProperties.Set(KafkaPrefix + nameof(produceResult.Topic), produceResult.Topic);
        context.ExtendedProperties.Set(KafkaPrefix + nameof(produceResult.Partition), produceResult.Partition);
        context.ExtendedProperties.Set(KafkaPrefix + nameof(produceResult.Offset), produceResult.Offset);
    }
}