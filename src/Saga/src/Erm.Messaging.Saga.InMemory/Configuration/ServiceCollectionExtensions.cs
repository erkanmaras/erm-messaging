using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Erm.Messaging.Saga;

namespace Erm.Messaging.Saga.InMemory;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static ISagaConfiguration UseInMemoryPersistence(this ISagaConfiguration configuration)
    {
        configuration.Services.AddSingleton<ISagaRepository, InMemorySagaRepository>();
        return configuration;
    }
}