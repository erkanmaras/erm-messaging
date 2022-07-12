using Microsoft.Extensions.DependencyInjection;
using Erm.Messaging;

namespace Erm.Messaging.Serialization.Protobuf;

public static class ProtobufMessageSerializerConfigurationExtensions
{
    //JsonOptions customization?
    public static void AddProtobuf(this MessageSerializationConfiguration configuration)
    {
        configuration.Register<ProtobufMessageSerializer>(ProtobufMessageSerializer.ContentTypeValue);
        configuration.ServiceCollection.AddSingleton<ProtobufMessageSerializer>();
    }
}