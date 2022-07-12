using System;
using JetBrains.Annotations;
using Erm.Messaging;

namespace Erm.Messaging.OutboxTransport;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static OutboxMessagingConfiguration AddOutboxTransport(this MessagingConfiguration messagingConfiguration, Action<OutboxMessagingConfiguration> configure)
    {
        var configuration = new OutboxMessagingConfiguration(messagingConfiguration.ServiceCollection);
        configure(configuration);
        return configuration;
    }
}