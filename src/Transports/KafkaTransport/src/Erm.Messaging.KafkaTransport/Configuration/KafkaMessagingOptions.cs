namespace Erm.Messaging.KafkaTransport;

public class KafkaMessagingOptions
{
    public const string Section = "Kafka";

    public string[]? BrokerAddresses { get; set; }

    public ConsumerOption?[]? Consumers { get; set; }
    public ProducerOption? Producer { get; set; }
    public SecurityOption? Security { get; set; }

    public class SecurityOption
    {
        public SecurityProtocol? SecurityProtocol { get; set; }
    }

    public class ConsumerOption
    {
        public string[]? Topics { get; set; }
        public string? Name { get; set; }
        public string? GroupId { get; set; }
        public int? WorkerCount { get; set; }
        public int? WorkerBufferCount { get; set; }
    }

    public class ProducerOption
    {
        public Acks? Acks { get; set; }
        public bool? Idempotence { get; set; }

        public int? LingerMs { get; set; }
        public int? Retries { get; set; }
        public int? RetryBackoff { get; set; }
    }
}