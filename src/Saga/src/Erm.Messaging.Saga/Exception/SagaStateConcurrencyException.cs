using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Erm.Messaging.Saga;

[PublicAPI]
public class SagaStateConcurrencyException : SagaException
{
    public SagaStateConcurrencyException(string message) : base(message)
    {
    }

    public SagaStateConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected SagaStateConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}