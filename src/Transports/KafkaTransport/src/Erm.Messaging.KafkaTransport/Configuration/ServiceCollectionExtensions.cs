using System;
using JetBrains.Annotations;
using Erm.KafkaClient;

namespace Erm.Messaging.KafkaTransport;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static void AddKafkaTransport(this MessagingConfiguration messagingConfiguration, Action<KafkaMessagingConfiguration> configure)
    {
        var configuration = new KafkaMessagingConfiguration(messagingConfiguration.ServiceCollection);
        configure(configuration);
        KafkaClientConfigurator.Configure(messagingConfiguration.ServiceCollection, configuration);
    }

    public static MessagingKafkaContext CreateMessagingKafkaContext(this IServiceProvider provider)
    {
        return new MessagingKafkaContext(provider.CreateKafkaClient());
    }
}