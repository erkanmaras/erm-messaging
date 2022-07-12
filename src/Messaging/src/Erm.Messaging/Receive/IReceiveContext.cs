using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public interface IReceiveContext : IMessageContext
{
    public IMessageEnvelope MessageEnvelope { get; }
    ReceiveResult Result { get; }
    Task<SendResult> Send<TMessage>(TMessage message) where TMessage : class;
    Task<SendResult> Send<TMessage>(Envelope<TMessage> envelope) where TMessage : class;
    Task<SendResult> Respond<TMessage>(Envelope<TMessage> envelope) where TMessage : class;
    Task<SendResult> Respond<TMessage>(TMessage message) where TMessage : class;
}