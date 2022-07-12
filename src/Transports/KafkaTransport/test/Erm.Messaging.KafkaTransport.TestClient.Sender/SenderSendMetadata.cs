using Erm.Messaging.KafkaTransport.TestClient.Shared;
using The.Smart.House.Receiver;
using The.Smart.House.Sender;

namespace Erm.Messaging.KafkaTransport.TestClient.Sender;

// ReSharper disable once ClassNeverInstantiated.Global
internal class SenderSendMetadata : SendMetadata
{
    private readonly TestContext _testContext;

    //
    public SenderSendMetadata(TestContext testContext)
    {
        _testContext = testContext;
        AddSendMetadata<RoomEnlightened>(GetDestination("the.smart.house.sender.event"));
        AddSendMetadata<TurnOnLights>(GetDestination("the.smart.house.receiver.command"));
        AddSendMetadata<AreLightsOn>(GetDestination("the.smart.house.receiver.query"));
    }

    //
    private string GetDestination(string destination)
    {
        return $"{_testContext.UniqueKeyForCurrentTestContext}.{destination}";
    }
}