using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Erm.Messaging.Saga.InMemory.Tests;

public class ConfigurationTests
{
    [Fact]
    public void UseInMemoryPersistence_Register_InMemorySagaRepository_As_Singleton()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMessaging(configuration => { configuration.AddSaga(sagaConfiguration => sagaConfiguration.UseInMemoryPersistence()); });
        serviceCollection.BuildServiceProvider().GetRequiredService<ISagaRepository>().Should().BeOfType<InMemorySagaRepository>();
        serviceCollection.Should().Contain(s => s.ServiceType == typeof(ISagaRepository) &&
                                                s.Lifetime == ServiceLifetime.Singleton);
    }
}