using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public interface ISendTransport
{
    public Task Send(ISendContext context, IMessageEnvelope envelope);
}

[PublicAPI]
public class NullSendTransport : ISendTransport
{
    public Task Send(ISendContext context, IMessageEnvelope envelope)
    {
        throw new InvalidOperationException("There is no service registered for ISendTransport!");
    }
}