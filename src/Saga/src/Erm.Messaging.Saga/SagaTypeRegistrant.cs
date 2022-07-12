using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging.Saga;

public static class SagaTypeRegistrant
{
    private static readonly Type SagaType = typeof(ISaga);

    public static void RegisterSagaTypes(IServiceCollection serviceCollection, IEnumerable<Assembly> assembliesToScan)
    {
        var sagaActionInterfaces = new[] { typeof(ISagaStartAction<>), typeof(ISagaAction<>) };
        foreach (var assembly in assembliesToScan)
        {
            var sagaTypes = assembly.GetTypes().Where(type => SagaType.IsAssignableFrom(type) && !type.IsInterface);
            foreach (var sagaType in sagaTypes)
            {
                foreach (var @interface in GetInterfaces(sagaType, includeInherited: false))
                {
                    if (@interface.IsGenericType)
                    {
                        var genericTypeDefinition = @interface.GetGenericTypeDefinition();
                        if (sagaActionInterfaces.Any(sagaActionInterface => sagaActionInterface == genericTypeDefinition))
                        {
                            serviceCollection.AddTransient(@interface, sagaType);
                        }
                    }
                }
            }
        }
    }

    private static IEnumerable<Type> GetInterfaces(Type type, bool includeInherited)
    {
        if (includeInherited || type.BaseType is null)
        {
            return type.GetInterfaces();
        }

        return type.GetInterfaces().Except(type.BaseType.GetInterfaces());
    }
}