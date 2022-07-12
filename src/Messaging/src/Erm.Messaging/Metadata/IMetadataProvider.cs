using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public interface IMetadataProvider
{
    string MessageDomain { get; }

    MessageType GetMessageType(string messageName);

    Type? GetMessageObjectType(string messageName);

    IEnumerable<Type> GetMessageObjectTypes();

    string GetDestinationAddress(string messageName);

    string GetContentType(string messageName);

    string GetReplyToAddress(string messageName);

    void AddSendMetadata<TMessage>(string destination, string contentType);

    void AddEventType<TMessage>();

    void AddCommandType<TMessage>();

    void AddQueryType<TMessage>();

    void AddMessageType<TMessage>(MessageType messageType);

    void AddMessageType(Type type, MessageType messageType);
}