using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Erm.Messaging.Saga.Tests;

public class SagaLocatorTests
{
    [Fact]
    public void Locate_ShouldReturnSagaActions_ForGivenMessageType()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(typeof(ISagaAction<SagaEvent>), typeof(FooSaga));
        serviceCollection.AddTransient(typeof(ISagaAction<SagaEvent>), typeof(BarSaga));

        using var scope = serviceCollection.BuildServiceProvider().CreateScope();

        var sagaLocator = new SagaLocator();
        var actions = sagaLocator.Locate<SagaEvent>(scope.ServiceProvider).ToList();

        actions.Count.Should().Be(2);
        actions.First().Should().BeOfType<FooSaga>();
        actions.Last().Should().BeOfType<BarSaga>();
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class SagaEvent
    {
    }

    private class FooSaga : Saga, ISagaStartAction<SagaEvent>
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

    private class BarSaga : Saga, ISagaStartAction<SagaEvent>
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
}