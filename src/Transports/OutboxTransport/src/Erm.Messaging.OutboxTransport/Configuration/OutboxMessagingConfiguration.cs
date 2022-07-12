using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Erm.Messaging;

namespace Erm.Messaging.OutboxTransport;

[PublicAPI]
public class OutboxMessagingConfiguration
{
    private readonly IServiceCollection _serviceCollection;

    public OutboxMessagingConfiguration(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public void AddSendTransport(int expiryDay)
    {
        //TODO:Outbox transport parametreleri ayarlanmalı.
        _serviceCollection.AddTransient<ISendTransport, OutboxSendTransport>();
    }
}