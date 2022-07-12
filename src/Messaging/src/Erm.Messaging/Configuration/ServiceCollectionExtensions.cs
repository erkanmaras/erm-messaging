using System;
using System.Linq;
using Erm.Messaging.Pipeline;
using Erm.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Erm.Core;
using Erm.Messaging.Serialization.Json;

namespace Erm.Messaging;

public static class ServiceCollectionExtensions
{
    public static void AddMessaging(this IServiceCollection serviceCollection, Action<MessagingConfiguration> configure)
    {
        var configuration = new MessagingConfiguration(serviceCollection);
        configure(configuration);

        //Receive
        BuildReceivePipeline(serviceCollection, configuration);
        serviceCollection.TryAddTransient<IReceiveEndpoint, ReceiveEndpoint>();

        //Send
        BuildSendPipeline(serviceCollection, configuration);
        serviceCollection.TryAddTransient<IMessageSender, MessageSender>();
        serviceCollection.TryAddTransient<ISendEndpoint, SendEndpoint>();

        //Serialization
        BuildSerialization(serviceCollection, configuration);

        //Metadata
        BuildMetadata(serviceCollection, configuration);

        //Others
        serviceCollection.AddSystemClock();

        //NullObjects

        //Some classes dependent to ISendTransport. if not any SendTransport registered register null transport.
        serviceCollection.TryAddTransient<ISendTransport, NullSendTransport>();
    }

    private static void BuildSerialization(IServiceCollection serviceCollection, MessagingConfiguration messagingConfiguration)
    {
        messagingConfiguration.Serialization(
            serialization => { serialization.AddJson(); }
        );
        serviceCollection.AddSingleton<IMessageSerializerFactory, MessageSerializerFactory>();
    }

    private static void BuildMetadata(IServiceCollection serviceCollection, MessagingConfiguration messagingConfiguration)
    {
        var metadataConfiguration = messagingConfiguration.MessageMetadataConfiguration;

        metadataConfiguration.MessageTypeConvention ??= typeof(MessageTypeConvention);
        serviceCollection.AddTransient(typeof(IMessageTypeConvention), metadataConfiguration.MessageTypeConvention);

        metadataConfiguration.SendMetadata ??= typeof(SendMetadata);
        serviceCollection.AddTransient(typeof(SendMetadata), metadataConfiguration.SendMetadata);

        metadataConfiguration.ReceiveMetadata ??= typeof(ReceiveMetadata);
        serviceCollection.AddTransient(typeof(ReceiveMetadata), metadataConfiguration.ReceiveMetadata);

        serviceCollection.AddSingleton<IMetadataProvider, MetadataProvider>(serviceProvider => new MetadataProvider(
            metadataConfiguration.MessageDomain,
            metadataConfiguration.DefaultSendContentType,
            metadataConfiguration.ScanMessagesIn,
            serviceProvider.GetService<IMessageTypeConvention>(),
            serviceProvider.GetService<SendMetadata>()
        ));
    }

    private static void BuildReceivePipeline(IServiceCollection serviceCollection, MessagingConfiguration configuration)
    {
        serviceCollection.AddSingleton<IMessagePipeline<IReceiveContext>>(provider =>
        {
            var middlewares = configuration.ReceivePipelineConfiguration.Middlewares.Select(factory => factory(provider)).ToList();
            return new MessagePipeline<IReceiveContext>(middlewares);
        });

        serviceCollection.AddTransient<IPipelineExecutor<IReceiveContext>, MessagePipelineExecutor<IReceiveContext>>();
    }

    private static void BuildSendPipeline(IServiceCollection serviceCollection, MessagingConfiguration configuration)
    {
        serviceCollection.AddSingleton<IMessagePipeline<ISendContext>>(provider =>
        {
            var middlewares = configuration.SendPipelineConfiguration.Middlewares.Select(factory => factory(provider)).ToList();
            return new MessagePipeline<ISendContext>(middlewares);
        });

        serviceCollection.AddTransient<IPipelineExecutor<ISendContext>, MessagePipelineExecutor<ISendContext>>();
    }
}