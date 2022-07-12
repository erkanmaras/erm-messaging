using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Erm.Messaging.Outbox.MySql.Tests.Configuration;

public class ServiceCollectionExtensionsTest
{
    [Fact]
    public void AddInMemoryMessageOutbox_ShouldRegister_InMemoryMessageOutbox_AsSingleton_To_ServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMySqlMessageOutbox(() => string.Empty);
        var service = serviceCollection.SingleOrDefault(x => x.ImplementationType == typeof(MySqlMessageOutbox));

        service.Should().NotBeNull();
        service?.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }
}