using System;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Erm.KafkaClient;
using Erm.KafkaClient.Configuration;

namespace Erm.Messaging.KafkaTransport;

internal static class KafkaClientConfigurator
{
    public static void Configure(IServiceCollection services, KafkaMessagingConfiguration messagingConfiguration)
    {
        services.AddKafkaClient(
            kafka => kafka
                .AddCluster(
                    cluster =>
                    {
                        cluster.WithBrokers(messagingConfiguration.BrokerAddresses);

                        if (messagingConfiguration.SecurityProtocol != null)
                        {
                            cluster.WithSecurityInformation(securityInformation => { securityInformation.SecurityProtocol = ToConfluentSecurityProtocol(messagingConfiguration.SecurityProtocol.Value); });
                        }

                        if (messagingConfiguration.ReceiveConfiguration != null)
                        {
                            foreach (var consumer in messagingConfiguration.ReceiveConfiguration.ConsumerConfigurations)
                            {
                                AddConsumer(cluster, consumer);
                            }
                        }

                        if (messagingConfiguration.SendConfiguration != null)
                        {
                            var transportType = messagingConfiguration.SendConfiguration.SendTransport ?? typeof(KafkaSendTransport);
                            services.AddTransient(typeof(ISendTransport), transportType);
                            AddProducer(cluster, messagingConfiguration.SendConfiguration.ProducerConfiguration);
                        }
                    })
        );
    }

    private static void AddConsumer(IClusterConfigurationBuilder clusterConfigurationBuilder, KafkaConsumerConfiguration consumerConfiguration)
    {
        if (clusterConfigurationBuilder == null)
        {
            throw new ArgumentNullException(nameof(clusterConfigurationBuilder));
        }

        if (consumerConfiguration == null)
        {
            throw new ArgumentNullException(nameof(consumerConfiguration));
        }

        //TODO:Consumer configuration tamamlanacak!
        //* Default değerler ne olmalı ?
        //* Hangi değerler parametrik olmalı ?
        clusterConfigurationBuilder.AddConsumer(
            consumer => consumer
                .Topics(consumerConfiguration.TopicNames)
                .WithGroupId(consumerConfiguration.GroupId)
                .WithName(consumerConfiguration.Name)
                .WithBufferSize(10) // ? consumer worker buffer size.
                .WithWorkersCount(consumerConfiguration.WorkersCount)
                //https://strimzi.io/blog/2021/01/07/consumer-tuning/
                .WithSessionTimeoutMs(10 * 1000) //  If no heartbeats are received in 10 sescond by the broker before the expiration of this session timeout, then the broker will remove this client from the group and initiate a rebalance. 
                .WithMaxPollIntervalMs(20 * 1000) // if consumer not call pool in 20 second , the consumer is considered to be failed , causing a rebalance
                .WithAutoCommitIntervalMs(2 * 1000) // commit stored offsets every 2 seconds
                .WithManualStoreOffsets()
                .WithAutoOffsetReset(AutoOffsetReset.Earliest) // start from first unread message
                .WithHandler<KafkaReceiveTransport>()
        );
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private static void AddProducer(IClusterConfigurationBuilder clusterConfigurationBuilder, KafkaProducerConfiguration? producerConfiguration)
    {
        if (clusterConfigurationBuilder == null)
        {
            throw new ArgumentNullException(nameof(clusterConfigurationBuilder));
        }

        if (producerConfiguration == null)
        {
            throw new ArgumentNullException(nameof(producerConfiguration));
        }

        clusterConfigurationBuilder.AddProducer(
            nameof(ISendTransport),
            producer =>
            {
                producer.WithIdempotence(producerConfiguration.Idempotence);
                producer.WithAcks(ToConfluentAck(producerConfiguration.Acks));

                if (producerConfiguration.Retries > 0)
                {
                    producer.WithMaxRetries(producerConfiguration.Retries.Value);
                }

                if (producerConfiguration.RetryBackoff > 0)
                {
                    producer.WithRetryBackoffMs(producerConfiguration.RetryBackoff.Value);
                }
            });
    }

    private static Confluent.Kafka.Acks ToConfluentAck(Acks acks)
    {
        return acks switch
        {
            Acks.None => Confluent.Kafka.Acks.None,
            Acks.Leader => Confluent.Kafka.Acks.Leader,
            Acks.All => Confluent.Kafka.Acks.All,
            _ => throw new ArgumentOutOfRangeException(nameof(acks))
        };
    }

    private static Confluent.Kafka.SecurityProtocol ToConfluentSecurityProtocol(SecurityProtocol securityProtocol)
    {
        return securityProtocol switch
        {
            SecurityProtocol.Plaintext => Confluent.Kafka.SecurityProtocol.Plaintext,
            SecurityProtocol.Ssl => Confluent.Kafka.SecurityProtocol.Ssl,
            SecurityProtocol.SaslPlaintext => Confluent.Kafka.SecurityProtocol.SaslPlaintext,
            SecurityProtocol.SaslSsl => Confluent.Kafka.SecurityProtocol.SaslSsl,
            _ => throw new ArgumentOutOfRangeException(nameof(securityProtocol))
        };
    }
}