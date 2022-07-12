using The.Smart.House.Receiver;
using The.Smart.House.Sender;

namespace Erm.Messaging.KafkaTransport.TestClient.Receiver;

public class ReceiveMessageTypeConvention : IMessageTypeConvention
{
    public bool IsEvent(Type type)
    {
        return new[] { nameof(RoomEnlightened) }.Contains(type.Name, StringComparer.InvariantCultureIgnoreCase);
    }

    public bool IsCommand(Type type)
    {
        return new[] { nameof(TurnOnLights) }.Contains(type.Name, StringComparer.InvariantCultureIgnoreCase);
    }

    public bool IsCommandResponse(Type type)
    {
        return new[] { nameof(TurnOnLightsResponse) }.Contains(type.Name, StringComparer.InvariantCultureIgnoreCase);
    }

    public bool IsQuery(Type type)
    {
        return new[] { nameof(AreLightsOn) }.Contains(type.Name, StringComparer.InvariantCultureIgnoreCase);
    }

    public bool IsQueryResponse(Type type)
    {
        return new[] { nameof(AreLightsOnResponse) }.Contains(type.Name, StringComparer.InvariantCultureIgnoreCase);
    }
}