using Erm.Messaging.KafkaTransport.TestClient.Shared;
using Erm.Messaging.TypedMessageHandler;
using The.Smart.House.Receiver;
using The.Smart.House.Sender;

// ReSharper disable ArgumentsStyleOther

namespace Erm.Messaging.KafkaTransport.TestClient.Receiver.Handlers;

// ReSharper disable once UnusedType.Global
public class ReceiverMessageHandler :
    IMessageHandler<TurnOnLights>,
    IMessageHandler<RoomEnlightened>,
    IMessageHandler<AreLightsOn>
{
    private readonly TestContext _testContext;
    private readonly IMessageSender _messageSender;

    public ReceiverMessageHandler(TestContext testContext, IMessageSender messageSender)
    {
        _testContext = testContext;
        _messageSender = messageSender;
    }

    public async Task Handle(IReceiveContext context, IEnvelope<TurnOnLights> envelope)
    {
        _testContext.Store.Add(("ReceivedMessage", envelope));

        await _messageSender.Send(new Envelope<TurnOnLightsResponse>(new TurnOnLightsResponse()
        )
        {
            Destination = envelope.Source + ".command-response",
            RequestId = envelope.MessageId
        });
    }

    public Task Handle(IReceiveContext context, IEnvelope<RoomEnlightened> envelope)
    {
        _testContext.Store.Add(("ReceivedMessage", envelope));
        return Task.CompletedTask;
    }

    public async Task Handle(IReceiveContext context, IEnvelope<AreLightsOn> envelope)
    {
        _testContext.Store.Add(("ReceivedMessage", envelope));

        await context.Respond(new Envelope<AreLightsOnResponse>(new AreLightsOnResponse()
        ));
    }
}