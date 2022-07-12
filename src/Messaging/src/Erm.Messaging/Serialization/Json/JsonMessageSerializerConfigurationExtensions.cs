using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging.Serialization.Json;

public static class JsonMessageSerializerConfigurationExtensions
{
    //JsonOptions customization?
    public static void AddJson(this MessageSerializationConfiguration configuration)
    {
        configuration.Register<JsonMessageSerializer>(MessageContentTypes.Json);
        configuration.ServiceCollection.AddSingleton<JsonMessageSerializer>();
    }
}