using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Erm.Messaging;

namespace Erm.Messaging.Saga;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static MessagingConfiguration AddSaga(this MessagingConfiguration messagingConfiguration, Action<ISagaConfiguration> configure)
    {
        messagingConfiguration.ServiceCollection.AddTransient<ISagaCoordinator, SagaCoordinator>();
        messagingConfiguration.ServiceCollection.AddTransient<ISagaLocator, SagaLocator>();
        messagingConfiguration.ServiceCollection.AddTransient<ISagaInitializer, SagaInitializer>();
        messagingConfiguration.ServiceCollection.AddTransient<ISagaProcessor, SagaProcessor>();

        var configuration = new SagaConfiguration(messagingConfiguration.ServiceCollection);
        configure(configuration);

        if (configuration.ScanSagasIn != null)
        {
            SagaTypeRegistrant.RegisterSagaTypes(messagingConfiguration.ServiceCollection, configuration.ScanSagasIn);
        }

        return messagingConfiguration;
    }

    public static void UseSaga(this ReceivePipelineConfiguration configuration)
    {
        configuration.Use(provider => new SagaMessageHandlerMiddleware(provider.GetRequiredService<ISagaCoordinator>()));
    }
}