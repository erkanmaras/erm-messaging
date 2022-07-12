using JetBrains.Annotations;

namespace Erm.Messaging.KafkaTransport;

[PublicAPI]
public class KafkaProducerConfiguration
{
    internal KafkaProducerConfiguration()
    {
    }

    public Acks Acks { get; set; } = Acks.All;
    public bool Idempotence { get; set; } = true;

    public int? LingerMs { get; set; }
    public int? Retries { get; set; }
    public int? RetryBackoff { get; set; }
}