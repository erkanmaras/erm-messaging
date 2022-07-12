using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;

namespace Erm.Messaging.KafkaTransport.TestClient.Shared;

public static class KafkaHelper
{
    public static void CreateTopics(string brokerAddresses, IEnumerable<string> topics)
    {
        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = brokerAddresses }).Build())
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            foreach (var topicName in topics)
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

    public static string GetKafkaHostAddress(IConfiguration configuration)
    {
        const string envName = $"Kafka:BrokerAddress";
        var kafkaHostAddress = configuration[envName];

        if (string.IsNullOrWhiteSpace(kafkaHostAddress))
        {
            throw new InvalidOperationException($"Kafka host address not specified! Env Name: {envName}");
        }

        return kafkaHostAddress;
    }
}