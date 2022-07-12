using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Erm.Messaging.Saga.Tests;

public class SagaMessageHandlerMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldCall_SagaCoordinator_WithEventRuntimeType()
    {
        var sagaMock = new Mock<ISagaCoordinator>
        {
            CallBase = true
        };

        sagaMock.Setup(saga => saga.Process(It.IsAny<IReceiveContext>(), It.IsAny<IEnvelope<SagaStartEvent>>(), null, null));
        var message = new SagaStartEvent();
        //cast to interface type
        IEnvelope envelope = new Envelope<SagaStartEvent>(message);
        var middleware = new SagaMessageHandlerMiddleware(sagaMock.Object);
        await middleware.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>()), envelope, (_, _) => Task.CompletedTask);
        //check called with event runtime type ->SagaStartEvent
        sagaMock.Verify(sampleSaga => sampleSaga.Process(It.IsAny<IReceiveContext>(), It.IsAny<IEnvelope<SagaStartEvent>>(), null, null), Times.Once());
    }

    private class SagaStartEvent
    {
    }
}