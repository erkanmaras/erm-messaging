using System;
using System.Linq;

namespace Erm.Messaging.Saga;

internal static class SagaExtensions
{
    public static Type? GetSagaDataType(this ISaga saga)
    {
        return saga
            .GetType()
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISaga<>))
            ?.GetGenericArguments()
            .FirstOrDefault();
    }

    public static object? InvokeGeneric(this ISaga saga, string method, params object[] args)
    {
        return saga
            .GetType()
            .GetMethod(method, args.Select(arg => arg.GetType()).ToArray())
            ?.Invoke(saga, args);
    }
}