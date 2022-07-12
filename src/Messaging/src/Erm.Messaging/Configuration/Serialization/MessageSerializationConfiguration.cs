using Erm.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging;

public class MessageSerializationConfiguration
{
    public readonly IServiceCollection ServiceCollection;


    public MessageSerializationConfiguration(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }

    public void Register<TSerializer>(string contentType) where TSerializer : IMessageSerializer
    {
        MessageSerializerFactory.RegisterType<TSerializer>(contentType);
    }
}