using System;
using Erm.Messaging.TypedMessageHandler.Middleware;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Erm.Messaging;

namespace Erm.Messaging.TypedMessageHandler;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static MessagingConfiguration AddTypedMessageHandler(this MessagingConfiguration messagingConfiguration, Action<TypedMessageHandlerConfiguration> configure)
    {
        var configuration = new TypedMessageHandlerConfiguration();
        configure(configuration);

        var typeMap = new MessageHandlerTypeMap(messagingConfiguration.ServiceCollection);

        if (configuration.ScanMessageHandlersIn != null)
        {
            typeMap.AddFromAssemblies(configuration.ScanMessageHandlersIn);
        }

        messagingConfiguration.ServiceCollection.AddSingleton<IMessageHandlerTypeMap>(typeMap);
        return messagingConfiguration;
    }

    public static void UseTypedMessageHandler(this ReceivePipelineConfiguration configuration)
    {
        configuration.Use(provider => new TypedMessageHandlerMiddleware(provider.GetRequiredService<IMessageHandlerTypeMap>(), provider.GetRequiredService<ILogger<TypedMessageHandlerMiddleware>>()));
    }
}