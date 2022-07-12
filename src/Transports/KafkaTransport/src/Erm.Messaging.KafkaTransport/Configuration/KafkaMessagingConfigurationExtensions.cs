using System;
using Microsoft.Extensions.Configuration;

namespace Erm.Messaging.KafkaTransport;

public static class KafkaMessagingConfigurationExtensions
{
    public static void UseConfiguration(this KafkaMessagingConfiguration messagingConfiguration, KafkaMessagingOptions options)
    {
        messagingConfiguration.BrokerAddresses = options.BrokerAddresses;

        if (options.Security != null)
        {
            if (options.Security.SecurityProtocol != null)
            {
                messagingConfiguration.SecurityProtocol = options.Security.SecurityProtocol;
            }
            else
            {
                throw new InvalidOperationException("Invalid kafka configuration Kafka:Security:SecurityProtocol invalid!");
            }
        }

        if (options.Consumers != null)
        {
            messagingConfiguration.ReceiveConfiguration = new KafkaMessagingConfiguration.KafkaReceiveConfiguration();
            foreach (var consumerOption in options.Consumers)
            {
                if (consumerOption == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(consumerOption.Name))
                {
                    throw new InvalidOperationException("Invalid kafka configuration Kafka:Consumer:Name required!");
                }

                messagingConfiguration.ReceiveConfiguration.AddConsumer(consumer =>
                {
                    consumer.GroupId = consumerOption.GroupId;
                    if (consumerOption.WorkerCount > 0)
                    {
                        consumer.WorkersCount = consumerOption.WorkerCount.Value;
                    }

                    if (consumerOption.WorkerBufferCount > 0)
                    {
                        consumer.WorkerBufferCount = consumerOption.WorkerBufferCount.Value;
                    }
                }, consumerOption.Name, consumerOption.Topics ?? Array.Empty<string>());
            }
        }

        if (options.Producer != null)
        {
            messagingConfiguration.SendConfiguration = new KafkaMessagingConfiguration.KafkaSendConfiguration();
            var producerOption = options.Producer;

            if (producerOption.Idempotence.HasValue)
            {
                messagingConfiguration.SendConfiguration.ProducerConfiguration.Idempotence = producerOption.Idempotence.Value;
            }

            if (producerOption.Acks.HasValue)
            {
                messagingConfiguration.SendConfiguration.ProducerConfiguration.Acks = producerOption.Acks.Value;
            }

            if (producerOption.LingerMs > 0)
            {
                messagingConfiguration.SendConfiguration.ProducerConfiguration.LingerMs = producerOption.LingerMs.Value;
            }

            if (producerOption.Retries > 0)
            {
                messagingConfiguration.SendConfiguration.ProducerConfiguration.LingerMs = producerOption.Retries.Value;
            }

            if (producerOption.RetryBackoff > 0)
            {
                messagingConfiguration.SendConfiguration.ProducerConfiguration.LingerMs = producerOption.RetryBackoff.Value;
            }
        }
    }

    public static void UseConfiguration(this KafkaMessagingConfiguration messagingConfiguration, IConfiguration configuration)
    {
        var options = configuration.GetSection(KafkaMessagingOptions.Section).Get<KafkaMessagingOptions>();
        if (options == null)
        {
            throw new InvalidOperationException($"{KafkaMessagingOptions.Section} section was not found in configuration!");
        }

        messagingConfiguration.UseConfiguration(options);
    }
}