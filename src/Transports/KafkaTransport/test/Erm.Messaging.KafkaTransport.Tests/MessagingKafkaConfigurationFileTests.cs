using System;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Erm.Messaging.KafkaTransport;
using Xunit;

namespace Erm.Messaging.KafkaTransport.Tests;

public class MessagingKafkaConfigurationFileTests
{
    public MessagingKafkaConfigurationFileTests()
    {
        var configurationBuilder = new ConfigurationManager()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@"appsettings.json");
        _configuration = configurationBuilder.Build();
    }

    private readonly IConfiguration _configuration;

    [Fact]
    public void Complete_Config_Test()
    {
        var serviceCollection = new ServiceCollection();
        var expected = new KafkaMessagingConfiguration(serviceCollection)
        {
            BrokerAddresses = new[] { "address1", "address2" },
            ReceiveConfiguration = new KafkaMessagingConfiguration.KafkaReceiveConfiguration
            {
                ConsumerConfigurations =
                {
                    new KafkaConsumerConfiguration("consumer1", new[] { "topic1" }),
                    new KafkaConsumerConfiguration("consumer2", new[] { "topic1" })
                    {
                        GroupId = "consumer2group",
                        WorkersCount = 3,
                        WorkerBufferCount = 5
                    }
                }
            },
            SendConfiguration = new KafkaMessagingConfiguration.KafkaSendConfiguration
            {
                ProducerConfiguration =
                {
                    Acks = Acks.All,
                    Idempotence = true,
                    LingerMs = 50,
                    Retries = 3,
                    RetryBackoff = 100
                }
            },
            SecurityProtocol = SecurityProtocol.Plaintext
        };

        var options = _configuration.GetSection("KafkaComplete").Get<KafkaMessagingOptions>();
        var messagingConfiguration = new KafkaMessagingConfiguration(serviceCollection);
        messagingConfiguration.UseConfiguration(options);
        messagingConfiguration.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void OnlyProducer_Config_Test()
    {
        var serviceCollection = new ServiceCollection();
        var expected = new KafkaMessagingConfiguration(serviceCollection)
        {
            BrokerAddresses = new[] { "address1" },
            SendConfiguration = new KafkaMessagingConfiguration.KafkaSendConfiguration
            {
                ProducerConfiguration =
                {
                    Acks = Acks.All,
                    Idempotence = true,
                    LingerMs = 50,
                    Retries = 3,
                    RetryBackoff = 100
                }
            }
        };

        var options = _configuration.GetSection("KafkaOnlyProducer").Get<KafkaMessagingOptions>();
        var messagingConfiguration = new KafkaMessagingConfiguration(serviceCollection);
        messagingConfiguration.UseConfiguration(options);
        messagingConfiguration.ReceiveConfiguration.Should().BeNull();
        messagingConfiguration.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void OnlyConsumer_Config_Test()
    {
        var serviceCollection = new ServiceCollection();
        var expected = new KafkaMessagingConfiguration(serviceCollection)
        {
            BrokerAddresses = new[] { "address1" },
            ReceiveConfiguration = new KafkaMessagingConfiguration.KafkaReceiveConfiguration
            {
                ConsumerConfigurations =
                {
                    new KafkaConsumerConfiguration("consumer1", new[] { "topic1" })
                }
            }
        };

        var options = _configuration.GetSection("KafkaOnlyConsumer").Get<KafkaMessagingOptions>();
        var messagingConfiguration = new KafkaMessagingConfiguration(serviceCollection);
        messagingConfiguration.UseConfiguration(options);
        messagingConfiguration.SendConfiguration.Should().BeNull();
        messagingConfiguration.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ConsumerWithoutName_Config_Test()
    {
        var serviceCollection = new ServiceCollection();
        var options = _configuration.GetSection("KafkaConsumerWithoutName").Get<KafkaMessagingOptions>();
        var messagingConfiguration = new KafkaMessagingConfiguration(serviceCollection);
        messagingConfiguration.Invoking(m => m.UseConfiguration(options)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Empty_Config_Test()
    {
        var configurationBuilder = new ConfigurationManager();
        var emptyConfiguration = configurationBuilder
            .AddJsonFile("non-existing.json", optional: true)
            .Build();

        var serviceCollection = new ServiceCollection();
        var messagingConfiguration = new KafkaMessagingConfiguration(serviceCollection);
        messagingConfiguration.Invoking(m => m.UseConfiguration(emptyConfiguration)).Should().Throw<InvalidOperationException>();
    }
}