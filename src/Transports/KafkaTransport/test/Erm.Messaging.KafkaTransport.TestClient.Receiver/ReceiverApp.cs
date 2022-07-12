using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Erm.Messaging.KafkaTransport.TestClient.Shared;
using Erm.Messaging.TypedMessageHandler;

#nullable disable

namespace Erm.Messaging.KafkaTransport.TestClient.Receiver;

[PublicAPI]
public class ReceiverApp
{
    private readonly IConfiguration _configuration;
    private readonly string _uniqueKeyForCurrentTestContext;
    private static TimeSpan _defaultDelay = TimeSpan.FromSeconds(10);

    private IHost _host;

    private bool _started;
    private bool _stopped;

    private TestContext _testContext;

    public ReceiverApp(IConfiguration configuration, string uniqueKeyForCurrentTestContext)
    {
        _configuration = configuration;
        _uniqueKeyForCurrentTestContext = uniqueKeyForCurrentTestContext;
    }

    public void RunHost(CancellationToken cancellationToken)
    {
        if (_started)
        {
            return;
        }

        _started = true;

        // Create topics with isolated names.
        var topics = new[]
        {
            $"{_uniqueKeyForCurrentTestContext}.the.smart.house.sender.event",
            $"{_uniqueKeyForCurrentTestContext}.the.smart.house.receiver.command",
            $"{_uniqueKeyForCurrentTestContext}.the.smart.house.receiver.query"
        };

        var kafkaHostAddress = KafkaHelper.GetKafkaHostAddress(_configuration);

        KafkaHelper.CreateTopics(kafkaHostAddress, topics);

        var builder = Host
            .CreateDefaultBuilder()
            .ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton(_ => new TestContext(_uniqueKeyForCurrentTestContext));

                serviceCollection.AddMessaging(messaging =>
                {
                    messaging.AddKafkaTransport(kafka =>
                        {
                            kafka.BrokerAddresses = new[] { kafkaHostAddress };
                            kafka.AddReceiveTransport(handler => { handler.AddConsumer(_ => { }, "receiver-app", topics); });

                            kafka.AddSendTransport(_ => { });
                        }
                    );
                    messaging.Metadata(metaData =>
                    {
                        metaData.ScanMessagesIn = new[] { GetType().Assembly };
                        metaData.UseMessageTypeConvention<ReceiveMessageTypeConvention>();
                        metaData.MessageDomain = $"{_uniqueKeyForCurrentTestContext}.the.smart.house.receiver";
                    });

                    messaging.AddTypedMessageHandler(typedMessageHandler => { typedMessageHandler.ScanMessageHandlersIn = new[] { typeof(ReceiverApp).Assembly }; });
                    messaging.ReceivePipeline(pipeline => { pipeline.UseTypedMessageHandler(); });
                    messaging.SendPipeline(pipeline => { pipeline.UseSendTransport(); });
                });
            });

        _host = builder.Build();
        _host.Start();
    }

    public async Task Stop()
    {
        if (_stopped)
        {
            return;
        }

        _stopped = true;
        await _host.StopAsync();
    }

    public TestContext TestContext => _testContext ??= _host.Services.GetRequiredService<TestContext>();

    public async Task SendMessage(object message)
    {
        var messageSender = _host.Services.GetRequiredService<IMessageSender>();
        await messageSender.Send(message);
    }
}