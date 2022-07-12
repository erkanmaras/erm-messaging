using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Erm.Messaging;
using Erm.Messaging.MessageGateway;

namespace Erm.Messaging.MessageGateway.InMemory;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static MessagingConfiguration AddInMemoryMessageGateway(this MessagingConfiguration configuration)
    {
        configuration.ServiceCollection.AddTransient<IMessageStatusRegistry, InMemoryMessageStatusRegistry>();
        return configuration;
    }
}