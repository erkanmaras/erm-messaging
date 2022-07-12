using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public interface IMessageSender
{
    Task<SendResult> Send<TMessage>(Envelope<TMessage> envelope) where TMessage : class;
    Task<SendResult> Send<TMessage>(TMessage message) where TMessage : class;
}