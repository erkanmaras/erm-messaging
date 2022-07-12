using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Erm.Core;
using Erm.Messaging;

namespace Erm.Messaging.MessageGateway;

public static class ServiceCollectionExtensions
{
    public static void UseMessageGateway(this ReceivePipelineConfiguration receivePipelineConfiguration, Action<MessageGatewayConfiguration>? configure = null)
    {
        var configuration = new MessageGatewayConfiguration();
        configure?.Invoke(configuration);
        receivePipelineConfiguration.Use(provider =>
            new MessageStatusMiddleware(
                provider.GetRequiredService<IMessageStatusRegistry>(),
                provider.GetRequiredService<IClock>(),
                provider.GetRequiredService<ILogger<MessageStatusMiddleware>>()));

        receivePipelineConfiguration.Use(provider =>
            new MessageRetryMiddleware(
                configuration.MessageRetryOptions ?? MessageRetryConfiguration.Default,
                provider.GetRequiredService<ILogger<MessageRetryMiddleware>>()));
    }
}