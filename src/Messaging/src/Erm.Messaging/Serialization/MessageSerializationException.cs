using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Erm.Messaging.Serialization;

[PublicAPI]
public class MessageSerializationException : SerializationException
{
    public MessageSerializationException(string message) : base(message)
    {
    }

    public MessageSerializationException(string? message, Exception innerException) : base(message, innerException)
    {
    }
}