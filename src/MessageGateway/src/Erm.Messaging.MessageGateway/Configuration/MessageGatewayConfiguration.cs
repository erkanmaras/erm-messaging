using System;
using Erm.Messaging;

namespace Erm.Messaging.MessageGateway;

public class MessageGatewayConfiguration
{
    internal MessageRetryConfiguration? MessageRetryOptions { get; private set; }

    public void Retries(byte retryCount, byte backoffSeconds, Func<Exception, IEnvelope, byte, bool>? retryPolicy)
    {
        MessageRetryOptions = new MessageRetryConfiguration(retryCount, backoffSeconds, retryPolicy);
    }
}