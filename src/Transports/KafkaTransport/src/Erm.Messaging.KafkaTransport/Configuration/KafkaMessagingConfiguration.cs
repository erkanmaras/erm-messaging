using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging.KafkaTransport;

[PublicAPI]
public class KafkaMessagingConfiguration
{
    private readonly IServiceCollection _serviceCollection;

    internal KafkaMessagingConfiguration(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public string[]? BrokerAddresses { get; set; }
    public SecurityProtocol? SecurityProtocol { get; set; }

    internal KafkaReceiveConfiguration? ReceiveConfiguration { get; set; }
    internal KafkaSendConfiguration? SendConfiguration { get; set; }

    public void AddReceiveTransport(Action<KafkaReceiveConfiguration> configuration)
    {
        ReceiveConfiguration = new KafkaReceiveConfiguration();
        configuration.Invoke(ReceiveConfiguration);
    }

    public void AddSendTransport(Action<KafkaSendConfiguration>? configuration = null)
    {
        if (configuration == null)
        {
            return;
        }

        SendConfiguration = new KafkaSendConfiguration();
        configuration.Invoke(SendConfiguration);
    }

    public class KafkaReceiveConfiguration
    {
        internal List<KafkaConsumerConfiguration> ConsumerConfigurations { get; } = new();

        public KafkaReceiveConfiguration AddConsumer(Action<KafkaConsumerConfiguration> configuration, string name, string[] topicNames)
        {
            var consumerConfiguration = new KafkaConsumerConfiguration(name, topicNames);
            configuration.Invoke(consumerConfiguration);
            SetConsumerConfigurationDefaults(consumerConfiguration);
            ConsumerConfigurations.Add(consumerConfiguration);
            return this;
        }

        private static void SetConsumerConfigurationDefaults(KafkaConsumerConfiguration consumerConfiguration)
        {
            consumerConfiguration.GroupId ??= $"{consumerConfiguration.Name}.group";
        }
    }

    public class KafkaSendConfiguration
    {
        public KafkaSendConfiguration()
        {
            ProducerConfiguration = new KafkaProducerConfiguration();
        }

        internal KafkaProducerConfiguration ProducerConfiguration { get; }

        internal Type? SendTransport;

        public void ConfigureProducer(Action<KafkaProducerConfiguration> configuration)
        {
            configuration.Invoke(ProducerConfiguration);
        }

        public void UseTransport<TTransport>() where TTransport : class, ISendTransport
        {
            SendTransport = typeof(TTransport);
        }
    }

    public enum Acks
    {
        None,
        Leader,
        All
    }
}