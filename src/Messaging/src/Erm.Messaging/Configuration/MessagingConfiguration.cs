using System;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging;

public class MessagingConfiguration
{
    public readonly IServiceCollection ServiceCollection;

    internal MessagingConfiguration(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
        ReceivePipelineConfiguration = new ReceivePipelineConfiguration();
        SendPipelineConfiguration = new SendPipelineConfiguration();
        MessageSerializationConfiguration = new MessageSerializationConfiguration(ServiceCollection);
        MessageMetadataConfiguration = new MessageMetadataConfiguration();
    }

    internal readonly ReceivePipelineConfiguration ReceivePipelineConfiguration;

    public void ReceivePipeline(Action<ReceivePipelineConfiguration> configure)
    {
        configure(ReceivePipelineConfiguration);
    }

    internal readonly SendPipelineConfiguration SendPipelineConfiguration;

    public void SendPipeline(Action<SendPipelineConfiguration> configure)
    {
        configure(SendPipelineConfiguration);
    }

    internal readonly MessageSerializationConfiguration MessageSerializationConfiguration;

    public void Serialization(Action<MessageSerializationConfiguration> configure)
    {
        configure(MessageSerializationConfiguration);
    }

    internal readonly MessageMetadataConfiguration MessageMetadataConfiguration;

    public void Metadata(Action<MessageMetadataConfiguration> configure)
    {
        configure(MessageMetadataConfiguration);
    }
}