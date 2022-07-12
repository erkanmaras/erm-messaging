using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Erm.Messaging.KafkaTransport;

namespace Erm.Messaging.Sample;

public static class KafkaClient
{
    public static void CreateTopicsIfNotExist(KafkaMessagingOptions options)
    {
        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = string.Join(",", options.BrokerAddresses!) }).Build())
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            foreach (var topicName in options.Consumers!.SelectMany(c => c!.Topics!))
            {
                if (metadata.Topics.Any(t => t.Topic.ToLowerInvariant().Equals(topicName)))
                {
                    continue;
                }

                try
                {
                    adminClient.CreateTopicsAsync(new[]
                    {
                        new TopicSpecification { Name = topicName, ReplicationFactor = 1, NumPartitions = 5 }
                    }).GetAwaiter().GetResult();
                }
                catch (CreateTopicsException e)
                {
                    Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                }
            }
        }
    }
}