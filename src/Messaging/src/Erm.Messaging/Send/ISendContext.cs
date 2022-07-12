using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public interface ISendContext : IMessageContext
{
    SendResult Result { get; }
}