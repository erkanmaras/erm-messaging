using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Erm.Core;
using Erm.Messaging.KafkaTransport.TestClient.Shared;
using Erm.Messaging.TypedMessageHandler;
using The.Smart.House.Receiver;
using The.Smart.House.Sender;

#nullable disable

namespace Erm.Messaging.KafkaTransport.TestClient.Sender;

[PublicAPI]
public class SenderApp
{
    private readonly IConfiguration _configuration;
    private readonly string _uniqueKeyForCurrentTestContext;
    private static TimeSpan _defaultDelay = TimeSpan.FromSeconds(10);

    private IHost _host;
    private TestContext _testContext;

    private bool _started;
    private bool _stopped;

    public SenderApp(IConfiguration configuration, string uniqueKeyForCurrentTestContext)
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
            $"{_uniqueKeyForCurrentTestContext}.the.smart.house.sender.command-response",
            $"{_uniqueKeyForCurrentTestContext}.the.smart.house.sender.query-response"
        };

        var kafkaHostAddress = KafkaHelper.GetKafkaHostAddress(_configuration);

        KafkaHelper.CreateTopics(kafkaHostAddress, topics);

        var builder = Host
            .CreateDefaultBuilder()
            .ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddMessaging(messaging =>
                {
                    messaging.AddKafkaTransport(kafka =>
                        {
                            kafka.BrokerAddresses = new[] { kafkaHostAddress };
                            kafka.AddReceiveTransport(receive =>
                            {
                                receive.AddConsumer(
                                    _ => { },
                                    "sender-app",
                                    topics);
                            });

                            kafka.AddSendTransport(_ => { });
                        }
                    );

                    messaging.Metadata(metaData =>
                    {
                        metaData.ScanMessagesIn = new[] { GetType().Assembly };
                        metaData.MessageDomain = $"{_uniqueKeyForCurrentTestContext}.the.smart.house.sender";
                        metaData.UseSendMetadata<SenderSendMetadata>();
                        metaData.UseMessageTypeConvention<SenderMessageTypeConvention>();
                    });

                    messaging.AddTypedMessageHandler(typedMessageHandler => { typedMessageHandler.ScanMessageHandlersIn = new[] { typeof(SenderApp).Assembly }; });
                    messaging.ReceivePipeline(pipeline => { pipeline.UseTypedMessageHandler(); });
                    messaging.SendPipeline(pipeline => { pipeline.UseSendTransport(); });
                });

                serviceCollection.AddSingleton(_ => new TestContext(_uniqueKeyForCurrentTestContext));
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

    public async Task<Guid> SendTurnOnLightsCommand()
    {
        var id = Uuid.Next();
        var messageSender = _host.Services.GetRequiredService<IMessageSender>();
        await messageSender.Send(new Envelope<TurnOnLights>(id, new TurnOnLights
        {
            Data = "Let there be light!"
        }));

        return id;
    }

    public async Task<Guid> PublishRoomEnlightenedEvent()
    {
        var id = Uuid.Next();

        var messageSender = _host.Services.GetRequiredService<IMessageSender>();
        await messageSender.Send(new Envelope<RoomEnlightened>(id, new RoomEnlightened()));

        return id;
    }

    public async Task<Guid> SendAreLightsOnQuery()
    {
        var id = Uuid.Next();
        var messageSender = _host.Services.GetRequiredService<IMessageSender>();
        await messageSender.Send(new Envelope<AreLightsOn>(id, new AreLightsOn
        {
            Data = "R.I.P. Tesla!"
        }));
        return id;
    }
}