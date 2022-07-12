using System;
using System.Threading.Tasks;
using FluentAssertions;
using Erm.Core;
using Xunit;

namespace Erm.Messaging.Saga.Tests;

public class SagaTests
{
    [Fact]
    public void Reject_ShouldSetStatus_ToRejected()
    {
        var saga = new FooSaga();
        saga.Reject(new Exception());
        saga.Status.Should().Be(SagaStatus.Rejected);
    }

    [Fact]
    public void Complete_ShouldSetStatus_ToCompleted()
    {
        var saga = new FooSaga();
        saga.Complete();
        saga.Status.Should().Be(SagaStatus.Completed);
    }

    [Fact]
    public void Initialize_ShouldSet_IdAndStatus()
    {
        var saga = new FooSaga();
        var sagaId = Uuid.Next();
        const SagaStatus status = SagaStatus.Pending;
        saga.Initialize(sagaId, status);
        saga.SagaId.Should().Be(sagaId);
        saga.Status.Should().Be(status);
    }


    private class FooSaga : Saga, ISagaStartAction<FooMessage>
    {
        public Task Handle(IReceiveContext context, IEnvelope<FooMessage> envelope)
        {
            return Task.CompletedTask;
        }

        public Task Compensate(IReceiveContext context, IEnvelope<FooMessage> envelope)
        {
            return Task.CompletedTask;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class FooMessage
    {
    }
}