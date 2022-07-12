using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging.TypedMessageHandler;

[PublicAPI]
public class MessageHandlerTypeMap : IMessageHandlerTypeMap
{
    private readonly ConcurrentDictionary<Type, List<Type>> _typeMap = new();
    private readonly IServiceCollection _serviceCollection;
    internal Type HandlerType = typeof(IMessageHandler<>);

    public MessageHandlerTypeMap(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IEnumerable<Type> GetMessageHandlerTypes(Type message)
    {
        _typeMap.TryGetValue(message, out var handlers);
        return handlers ?? Enumerable.Empty<Type>();
    }

    public void AddFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && !x.IsInterface).ToList();

            foreach (var type in types)
            {
                if (type.GetInterfaces().Any(@interface => @interface.IsGenericType && HandlerType == @interface.GetGenericTypeDefinition()))
                {
                    AddToMapAndRegister(type);
                }
            }
        }
    }

    public void Add(Type handler)
    {
        AddToMapAndRegister(handler);
    }

    public void AddRange(IEnumerable<Type> handlers)
    {
        foreach (var handlerType in handlers)
        {
            Add(handlerType);
        }
    }

    private void AddToMapAndRegister(Type handlerType)
    {
        var messageTypes = handlerType
            .GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == HandlerType)
            .Select(x => x.GenericTypeArguments[0]);

        foreach (var messageType in messageTypes)
        {
            var handlers = _typeMap.GetOrAdd(messageType, _ => new List<Type>());
            handlers.Add(handlerType);
        }

        _serviceCollection.Add(ServiceDescriptor.Describe(handlerType, handlerType, ServiceLifetime.Transient));
    }
}