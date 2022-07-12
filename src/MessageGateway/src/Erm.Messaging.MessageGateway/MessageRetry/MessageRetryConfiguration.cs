using System;
using Erm.Messaging;

namespace Erm.Messaging.MessageGateway;

public class MessageRetryConfiguration
{
    internal static readonly MessageRetryConfiguration Default = new(retryCount: 3, backoffDuration: 5);

    public MessageRetryConfiguration(byte retryCount, byte backoffDuration, Func<Exception, IEnvelope, byte, bool>? policy = null)
    {
        RetryCount = retryCount;
        BackoffDuration = backoffDuration;
        Policy = policy;
    }

    public byte RetryCount { get; }

    /// <summary>
    /// The duration in seconds.
    /// </summary>
    public byte BackoffDuration { get; }

    public Func<Exception, IEnvelope, byte, bool>? Policy { get; }
}