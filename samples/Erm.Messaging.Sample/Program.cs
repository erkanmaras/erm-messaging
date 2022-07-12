using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Erm.Core;
using Erm.Messaging.KafkaTransport;
using Erm.Messaging.MessageGateway;
using Erm.Messaging.MessageGateway.InMemory;
using Erm.Messaging.Outbox.InMemory;
using Erm.Messaging.Saga;
using Erm.Messaging.Saga.InMemory;
using Erm.Messaging.Serialization.Protobuf;
using Erm.Messaging.TypedMessageHandler;

namespace Erm.Messaging.Sample;

internal static class Program
{
    private static async Task Main()
    {
        var rand = new Random((int)DateTimeOffset.UtcNow.Ticks);

        var builder = Host
            .CreateDefaultBuilder()
            .ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSystemClock();
                serviceCollection.AddMessaging(messaging =>
                {
                    var config = serviceCollection.BuildServiceProvider().GetRequiredService<IConfiguration>();
                    var options = config.GetSection(KafkaMessagingOptions.Section).Get<KafkaMessagingOptions>();
                    KafkaClient.CreateTopicsIfNotExist(options);

                    messaging.Serialization(
                        serialization => { serialization.AddProtobuf(); }
                    );

                    messaging.Metadata(metaData =>
                    {
                        metaData.ScanMessagesIn = new[] { typeof(Program).Assembly };
                        metaData.MessageDomain = "ping-pong-game";
                        metaData.UseSendMetadata<PingPongSendMetadata>();
                        metaData.UseMessageTypeConvention<PingPongMessageConvention>();
                    });

                    messaging.AddKafkaTransport(kafka =>
                        {
                            kafka.UseConfiguration(config);
                            // kafka.BrokerAddresses = new[] { config["Kafka:HostAddress"] };
                            //
                            // kafka.AddReceiveTransport(receiveTransport =>
                            // {
                            //     receiveTransport.AddConsumer(
                            //         consumer => { consumer.WorkersCount = 3; },
                            //         "ping-consumer",
                            //         new[] { "ping-topic" });
                            //
                            //     receiveTransport.AddConsumer(
                            //         consumer => { consumer.WorkersCount = 3; },
                            //         "pong-consumer",
                            //         new[] { "pong-topic" });
                            // });
                            //
                            // kafka.AddSendTransport();
                        }
                    );

                    //messaging.AddOutboxTransport(outbox => { outbox.AddSendTransport(expiryDay: 3); });
                    messaging.AddInMemoryMessageGateway();
                    messaging.AddSaga(saga =>
                    {
                        saga.ScanSagasIn = new[] { typeof(Program).Assembly };
                        saga.UseInMemoryPersistence();
                    });

                    messaging.AddTypedMessageHandler(typedMessageHandler => { typedMessageHandler.ScanMessageHandlersIn = new[] { typeof(Program).Assembly }; });

                    messaging.ReceivePipeline(pipeline =>
                    {
                        pipeline.Use(async (context, envelope, next) =>
                        {
                            envelope.ExtendedProperties.Set("power", rand.Next(1, 9).ToString());
                            await next(context, envelope);
                        });

                        pipeline.UseMessageGateway();
                        pipeline.UseTypedMessageHandler();
                        pipeline.UseSaga();
                        pipeline.Use(_ => new SetPlayerMiddleware<IReceiveContext>());
                    });
                    messaging.SendPipeline(pipeline =>
                    {
                        pipeline.Use(async (context, envelope, next) =>
                        {
                            envelope.ExtendedProperties.Set("power", rand.Next(1, 9).ToString());
                            await next(context, envelope);
                        });

                        pipeline.Use(async (context, envelope, next) =>
                        {
                            envelope.ExtendedProperties.Set("stamp", rand.Next(1, 9).ToString());
                            await next(context, envelope);
                        });

                        pipeline.Use(_ => new SetPlayerMiddleware<ISendContext>());
                        pipeline.UseSendTransport();
                    });
                });

                serviceCollection.AddInMemoryMessageOutbox();
            });

        using var host = builder.Build();
        host.Start();

        var correlationId = Uuid.Next();
        var messageSender = host.Services.GetRequiredService<IMessageSender>();
        await messageSender.Send(new Envelope<PingCommand>(new PingCommand()) { CorrelationId = correlationId });

        Console.WriteLine("Press any key , stop game.");
        Console.ReadLine();

        await host.StopAsync();
    }
}