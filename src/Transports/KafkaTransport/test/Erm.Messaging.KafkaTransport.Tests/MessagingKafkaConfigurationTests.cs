using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Erm.KafkaClient.Consumers;
using Erm.KafkaClient.Producers;
using Erm.Core;
using Erm.Messaging.KafkaTransport;
using Xunit;

namespace Erm.Messaging.KafkaTransport.Tests;

public class MessagingKafkaConfigurationTests
{
    [Fact]
    public void Register_RequiredInterfaces_ToServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddSystemClock();

        serviceCollection.AddMessaging(messaging =>
        {
            messaging.Metadata(metaData => { metaData.ScanMessagesIn = new[] { typeof(MessagingKafkaConfigurationTests).Assembly }; });

            messaging.AddKafkaTransport(kafka =>
                {
                    kafka.BrokerAddresses = new[] { "localhost:9093" };
                    kafka.AddReceiveTransport(receive =>
                    {
                        receive.AddConsumer(
                            consumer => { consumer.WorkersCount = 10; },
                            "comm.order",
                            new[] { "era.comm.order.event" });
                    });
                    kafka.AddSendTransport(send => { send.ConfigureProducer(producer => producer.Acks = Acks.Leader); });
                }
            );

            messaging.SendPipeline(pipeline => { pipeline.UseSendTransport(); });
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        serviceProvider.GetService<IProducerAccessor>().Should().NotBeNull();
        serviceProvider.GetService<IConsumerAccessor>().Should().NotBeNull();
        serviceProvider.GetService<IProducerAccessor>()![nameof(ISendTransport)].Should().NotBeNull();
        serviceProvider.GetService<IMessageSender>().Should().NotBeNull();
    }
}