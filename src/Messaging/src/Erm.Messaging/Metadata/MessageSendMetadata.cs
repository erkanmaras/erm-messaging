using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class MessageSendMetadata
{
    public MessageSendMetadata(string messageName, string destination, string contentType)
    {
        MessageName = messageName;
        Destination = destination;
        ContentType = contentType;
    }

    public string MessageName { get; init; }
    public string Destination { get; init; }
    public string ContentType { get; init; }
}