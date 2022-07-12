using System.Reflection;

namespace Erm.Messaging.TypedMessageHandler;

public class TypedMessageHandlerConfiguration
{
    public Assembly[]? ScanMessageHandlersIn { get; set; }
}