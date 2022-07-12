using JetBrains.Annotations;

namespace Erm.Messaging.MessageGateway;

[PublicAPI]
public class MessageFaultDetails
{
    public MessageFaultDetails(string type, string detail)
    {
        Type = type;
        Detail = detail;
    }

    public string Type { get; set; }
    public string Detail { get; set; }
}