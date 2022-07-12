using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class MetadataProvider : IMetadataProvider
{
    private readonly ReceiveMetadata _receiveMetadata;
    private readonly IMessageTypeConvention _messageTypeConvention;
    private readonly SendMetadata _sendMetadata;

    public MetadataProvider(
        string? messageDomain = null,
        string? defaultSendContentType = null,
        IEnumerable<Assembly>? scanMessagesIn = null,
        IMessageTypeConvention? messageTypeConvention = null,
        SendMetadata? messageSendMetadata = null,
        ReceiveMetadata? messageReceiveMetadata = null)
    {
        MessageDomain = messageDomain ?? string.Empty;
        _messageTypeConvention = messageTypeConvention ?? new MessageTypeConvention();
        _sendMetadata = messageSendMetadata ?? new SendMetadata();
        _receiveMetadata = messageReceiveMetadata ?? new ReceiveMetadata();
        AddMessageTypesFromAssemblies(scanMessagesIn);
    }

    public string MessageDomain { get; }

    private readonly Dictionary<string, MessageType> _messageTypes = new();
    private readonly Dictionary<string, Type> _messageObjectTypes = new();


    public virtual Type? GetMessageObjectType(string messageName)
    {
        if (string.IsNullOrWhiteSpace(messageName))
        {
            throw new ArgumentException($"{nameof(messageName)} is null or empty!");
        }

        _messageObjectTypes.TryGetValue(messageName, out var objectType);
        return objectType;
    }

    public virtual IEnumerable<Type> GetMessageObjectTypes()
    {
        return _messageObjectTypes.Values;
    }

    public virtual string GetDestinationAddress(string messageName)
    {
        ArgumentNullException.ThrowIfNull(messageName);

        var metadata = _sendMetadata.GetSendMetadata(messageName);
        if (metadata != null && !string.IsNullOrWhiteSpace(metadata.Destination))
        {
            return metadata.Destination;
        }

        var messageType = GetMessageType(messageName);

        switch (messageType)
        {
            case MessageType.Event:
                return _sendMetadata.GetEventsDefaultDestination(MessageDomain);
            case MessageType.Command:
            case MessageType.Query:
                throw new InvalidOperationException($"Message destination not configured for {messageName}!");
            case MessageType.CommandResponse:
            case MessageType.QueryResponse:
            default:
                throw new InvalidOperationException($"Message destination not valid for {messageType}!");
        }
    }

    public virtual string GetContentType(string messageName)
    {
        var metadata = _sendMetadata.GetSendMetadata(messageName);
        return string.IsNullOrWhiteSpace(metadata?.ContentType) ? MessageContentTypes.Default : metadata.ContentType;
    }

    public virtual MessageType GetMessageType(string messageName)
    {
        if (_messageTypes.TryGetValue(messageName, out var messageType))
        {
            return messageType;
        }

        throw new InvalidOperationException($"MessageType not found for {messageName}!");
    }

    public virtual string GetReplyToAddress(string messageName)
    {
        ArgumentNullException.ThrowIfNull(messageName);
        if (!_messageTypes.TryGetValue(messageName, out var messageType))
        {
            return string.Empty;
        }

        return messageType switch
        {
            MessageType.Command => _receiveMetadata.GetCommandResponseDestination(MessageDomain),
            MessageType.Query => _receiveMetadata.GetQueryResponseDestination(MessageDomain),
            MessageType.Event => string.Empty,
            _ => string.Empty
        };
    }

    protected void AddMessageTypesFromAssemblies(IEnumerable<Assembly>? assemblies)
    {
        if (assemblies == null)
        {
            return;
        }

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && !x.IsInterface).ToList();
            foreach (var type in types)
            {
                if (TryResolveMessageTypeByConvention(type))
                {
                    _messageObjectTypes.TryAdd(type.FullName!, type);
                }
            }
        }
    }

    public void AddSendMetadata<TMessage>(string destination, string contentType)
    {
        _sendMetadata.AddSendMetadata<TMessage>(destination, contentType);
    }

    public void AddEventType<TMessage>()
    {
        AddMessageType(typeof(TMessage), MessageType.Event);
    }

    public void AddCommandType<TMessage>()
    {
        AddMessageType(typeof(TMessage), MessageType.Command);
    }

    public void AddQueryType<TMessage>()
    {
        AddMessageType(typeof(TMessage), MessageType.Query);
    }

    public void AddMessageType<TMessage>(MessageType messageType)
    {
        AddMessageType(typeof(TMessage), messageType);
    }

    public void AddMessageType(Type type, MessageType messageType)
    {
        _messageTypes.TryAdd(type.FullName!, messageType);
        _messageObjectTypes.TryAdd(type.FullName!, type);
    }

    private bool TryResolveMessageTypeByConvention(Type type)
    {
        if (_messageTypeConvention.IsEvent(type))
        {
            return _messageTypes.TryAdd(type.FullName!, MessageType.Event);
        }

        if (_messageTypeConvention.IsCommand(type))
        {
            return _messageTypes.TryAdd(type.FullName!, MessageType.Command);
        }

        if (_messageTypeConvention.IsCommandResponse(type))
        {
            return _messageTypes.TryAdd(type.FullName!, MessageType.CommandResponse);
        }

        if (_messageTypeConvention.IsQuery(type))
        {
            return _messageTypes.TryAdd(type.FullName!, MessageType.Query);
        }

        if (_messageTypeConvention.IsQueryResponse(type))
        {
            return _messageTypes.TryAdd(type.FullName!, MessageType.QueryResponse);
        }

        return false;
    }
}