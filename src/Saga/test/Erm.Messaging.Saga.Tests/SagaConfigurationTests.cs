using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Erm.Core;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ArrangeTypeMemberModifiers

namespace Erm.Messaging.Saga.Tests;

public class SagaConfigurationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SagaConfigurationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void AddSagaWithAssembliesToScanParameters_ShouldRegister_SagaTypes()
    {
        var serviceCollection = new ServiceCollection();

        _testOutputHelper.WriteLine("serviceCollection.Count: {0}", serviceCollection.Count);

        serviceCollection.AddMessaging(messaging => { messaging.AddSaga(saga => saga.ScanSagasIn = new[] { GetType().Assembly }); });

        _testOutputHelper.WriteLine("GetType().Assembly: {0}", GetType().Assembly);
        _testOutputHelper.WriteLine("serviceCollection.Count: {0}", serviceCollection.Count);

        serviceCollection.Should().Contain(s => s.ServiceType == typeof(ISagaAction<FooBarEvent>) && s.ImplementationType == typeof(FooSaga));
        serviceCollection.Should().Contain(s => s.ServiceType == typeof(ISagaAction<FooBarEvent>) && s.ImplementationType == typeof(BarSaga));
        serviceCollection.Should().Contain(s => s.ServiceType == typeof(ISagaStartAction<FooBarEvent>) && s.ImplementationType == typeof(FooSaga));
        serviceCollection.Should().Contain(s => s.ServiceType == typeof(ISagaStartAction<FooBarEvent>) && s.ImplementationType == typeof(BarSaga));
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal class FooBarEvent
{
    public FooBarEvent()
    {
        Version = "1";
        Id = Uuid.Next();
    }

    public string Version { get; }
    public Guid Id { get; }
}

internal class FooSaga : Saga, ISagaStartAction<FooBarEvent>
{
    public Task Handle(IReceiveContext context, IEnvelope<FooBarEvent> envelope)
    {
        return Task.CompletedTask;
    }

    public Task Compensate(IReceiveContext context, IEnvelope<FooBarEvent> envelope)
    {
        return Task.CompletedTask;
    }
}

internal class BarSaga : Saga, ISagaStartAction<FooBarEvent>
{
    public Task Handle(IReceiveContext context, IEnvelope<FooBarEvent> envelope)
    {
        return Task.CompletedTask;
    }

    public Task Compensate(IReceiveContext context, IEnvelope<FooBarEvent> envelope)
    {
        return Task.CompletedTask;
    }
}