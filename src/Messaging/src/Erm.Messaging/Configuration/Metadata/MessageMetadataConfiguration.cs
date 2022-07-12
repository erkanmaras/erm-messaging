using System;
using System.Reflection;

namespace Erm.Messaging;

public class MessageMetadataConfiguration
{
    public string MessageDomain { get; set; } = string.Empty;
    public string DefaultSendContentType { get; set; } = MessageContentTypes.Default;
    public Assembly[]? ScanMessagesIn { get; set; }

    internal Type? SendMetadata;
    internal Type? ReceiveMetadata;
    internal Type? MessageTypeConvention;

    public void UseMessageTypeConvention<T>() where T : IMessageTypeConvention
    {
        MessageTypeConvention = typeof(T);
    }

    public void UseSendMetadata<T>() where T : SendMetadata
    {
        SendMetadata = typeof(T);
    }

    public void UseReceiveMetadata<T>() where T : ReceiveMetadata
    {
        ReceiveMetadata = typeof(T);
    }
}