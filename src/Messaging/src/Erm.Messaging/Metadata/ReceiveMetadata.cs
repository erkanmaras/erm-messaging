using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class ReceiveMetadata
{
    public virtual string GetCommandResponseDestination(string messageDomain)
    {
        return $"{messageDomain}.command-response";
    }

    public virtual string GetQueryResponseDestination(string messageDomain)
    {
        return $"{messageDomain}.query-response";
    }
}