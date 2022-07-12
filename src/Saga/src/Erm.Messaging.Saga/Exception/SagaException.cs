using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Erm.Messaging.Saga;

[PublicAPI]
public class SagaException : Exception
{
    public SagaException(string message) : base(message)
    {
    }

    public SagaException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected SagaException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}