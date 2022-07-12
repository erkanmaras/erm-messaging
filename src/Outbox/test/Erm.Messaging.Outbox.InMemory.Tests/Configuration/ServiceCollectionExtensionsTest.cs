using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Erm.Core;
using Erm.MessageOutbox;
using Xunit;

namespace Erm.Messaging.Outbox.InMemory.Tests.Configuration;

public class ServiceCollectionExtensionsTest
{
    [Fact]
    public void AddInMemoryMessageOutbox_ShouldRegister_InMemoryMessageOutbox_AsSingleton_To_ServiceCollection()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IClock, Clock>();

        // Act
        serviceCollection.AddInMemoryMessageOutbox();

        // Assert
        var service = serviceCollection.SingleOrDefault(x => x.ServiceType == typeof(IMessageOutbox));

        service.Should().NotBeNull();
        service?.Lifetime.Should().Be(ServiceLifetime.Singleton);

        // Assert
        var provider = serviceCollection.BuildServiceProvider();
        var outbox = provider.GetRequiredService<IMessageOutbox>();

        outbox.Should().NotBeNull();
        outbox.GetType().Should().BeAssignableTo(typeof(InMemoryMessageOutbox));
    }
}