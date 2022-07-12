using JetBrains.Annotations;

namespace Erm.Messaging.Sample;

[UsedImplicitly]
public class PingPongSendMetadata : SendMetadata
{
    public PingPongSendMetadata()
    {
        AddSendMetadata<PingCommand>(destination: "ping-topic");
        AddSendMetadata<PongCommand>(destination: "pong-topic");
    }
}

public class PingPongMessageConvention : IMessageTypeConvention
{
    public bool IsEvent(Type type)
    {
        return false;
    }

    public bool IsCommand(Type type)
    {
        return new[] { nameof(PingCommand), nameof(PongCommand) }.Contains(type.Name, StringComparer.InvariantCultureIgnoreCase);
    }

    public bool IsCommandResponse(Type type)
    {
        return false;
    }

    public bool IsQuery(Type type)
    {
        return false;
    }

    public bool IsQueryResponse(Type type)
    {
        return false;
    }
}