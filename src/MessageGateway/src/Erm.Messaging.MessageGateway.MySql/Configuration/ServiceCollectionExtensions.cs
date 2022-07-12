using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Erm.Messaging;
using Erm.Messaging.MessageGateway;

namespace Erm.Messaging.MessageGateway.MySql;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static MessagingConfiguration AddMySqlMessageGateway(this MessagingConfiguration configuration, Func<string> connectionStringProvider)
    {
        configuration.ServiceCollection.AddTransient<ConnectionStringProvider>(_ => connectionStringProvider.Invoke);
        configuration.ServiceCollection.AddTransient<IMessageStatusRegistryMySqlConfiguration, MessageStatusRegistryMySqlConfiguration>();
        configuration.ServiceCollection.AddTransient<IMessageStatusRegistry, MySqlMessageStatusRegistry>();

        return configuration;
    }
}