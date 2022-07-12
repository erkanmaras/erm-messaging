using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Erm.Messaging.Saga.Tests;

public class SagaTypeRegistrantTests
{
    [Fact]
    public void RegisterSagaTypes_ShouldRegister_SagaTypesToServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        SagaTypeRegistrant.RegisterSagaTypes(serviceCollection, new[] { GetType().Assembly });
        serviceCollection.Should().Contain(s => s.ServiceType == typeof(ISagaAction<SagaEvent>) &&
                                                s.ImplementationType == typeof(MySaga) &&
                                                s.Lifetime == ServiceLifetime.Transient);
    }
}

[PublicAPI]
public class MySaga : Saga, ISagaAction<SagaEvent>
{
    public Task Handle(IReceiveContext context, IEnvelope<SagaEvent> envelope)
    {
        return Task.CompletedTask;
    }

    public Task Compensate(IReceiveContext context, IEnvelope<SagaEvent> envelope)
    {
        return Task.CompletedTask;
    }
}

public abstract class SagaEvent
{
}