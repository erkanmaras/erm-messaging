using Erm.Messaging.KafkaTransport.TestClient.Shared;
using Erm.Messaging.TypedMessageHandler;
using The.Smart.House.Receiver;

namespace Erm.Messaging.KafkaTransport.TestClient.Sender.Handlers;

// ReSharper disable once UnusedType.Global
public class SenderMessageHandler :
    IMessageHandler<TurnOnLightsResponse>,
    IMessageHandler<AreLightsOnResponse>
{
    private readonly TestContext _testContext;

    public SenderMessageHandler(TestContext testContext)
    {
        _testContext = testContext;
    }

    public Task Handle(IReceiveContext context, IEnvelope<TurnOnLightsResponse> envelope)
    {
        _testContext.Store.Add(("ReceivedMessage", envelope));
        return Task.CompletedTask;
    }

    public Task Handle(IReceiveContext context, IEnvelope<AreLightsOnResponse> envelope)
    {
        _testContext.Store.Add(("ReceivedMessage", envelope));
        return Task.CompletedTask;
    }
}