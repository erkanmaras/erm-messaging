using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class SendMetadata
{
    private Dictionary<string, MessageSendMetadata> SendMessageMetadata { get; } = new();

    protected virtual string GetDefaultContentType()
    {
        return MessageContentTypes.Default;
    }

    public virtual string GetEventsDefaultDestination(string messageDomain)
    {
        return $"{messageDomain}.events";
    }

    public virtual MessageSendMetadata? GetSendMetadata(string messageName)
    {
        if (string.IsNullOrWhiteSpace(messageName))
        {
            throw new ArgumentException($"{nameof(messageName)} is null or empty!");
        }

        if (SendMessageMetadata.TryGetValue(messageName, out var metadata))
        {
            return metadata;
        }

        return null;
    }

    public void AddSendMetadata<TMessage>(string destination, string? contentType = null)
    {
        var messageType = typeof(TMessage);
        TryAddSendMetadata(messageType.FullName!, destination, contentType ?? GetDefaultContentType());
    }

    private void TryAddSendMetadata(string messageName, string destination, string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(messageName))
        {
            throw new ArgumentException($"{nameof(messageName)} is null or empty!");
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentException($"{nameof(destination)} is null or empty!");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException($"{nameof(contentType)} is null or empty!");
        }

        SendMessageMetadata.TryAdd(messageName, new MessageSendMetadata(messageName, destination, contentType));
    }
}