using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging.Serialization;

[PublicAPI]
public class MessageSerializerFactory : IMessageSerializerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MessageSerializerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static readonly Dictionary<string, Type> ContentTypeSerializerMap = new();

    public static void RegisterType<TSerializer>(string contentType) where TSerializer : IMessageSerializer
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("ContentType empty!");
        }

        ContentTypeSerializerMap[contentType] = typeof(TSerializer);
    }

    public IMessageSerializer GetSerializer(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("ContentType empty!");
        }

        ContentTypeSerializerMap.TryGetValue(contentType, out var serializerType);

        if (serializerType == null)
        {
            throw new InvalidOperationException($"Serializer not registered for {contentType} content type!");
        }

        return (IMessageSerializer)_serviceProvider.GetRequiredService(serializerType);
    }
}