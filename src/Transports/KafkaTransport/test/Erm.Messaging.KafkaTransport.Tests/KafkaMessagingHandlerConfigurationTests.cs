using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Erm.Core;
using Erm.Messaging.KafkaTransport;
using Xunit;

namespace Erm.Messaging.KafkaTransport.Tests;

public class KafkaMessagingHandlerConfigurationTests
{
    [Fact]
    public void UseTransport_Should_Register_GivenTransport()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddSystemClock();
        serviceCollection.AddMessaging(messaging =>
        {
            messaging.AddKafkaTransport(kafka =>
                {
                    kafka.BrokerAddresses = new[] { "localhost:9093" };
                    kafka.AddSendTransport(sendTransport =>
                    {
                        sendTransport.UseTransport<FakeTransport>();
                        sendTransport.ConfigureProducer(producer => producer.Acks = Acks.Leader);
                    });
                }
            );
        });

        serviceCollection.Should().Contain(s => s.ServiceType == typeof(ISendTransport) && s.ImplementationType == typeof(FakeTransport));
    }

    private class FakeTransport : ISendTransport
    {
        private readonly Mock<ISendTransport> _mock = new();

        public Task Send(ISendContext context, IMessageEnvelope envelope)
        {
            return _mock.Object.Send(context, envelope);
        }
    }
}