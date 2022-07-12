using System.Threading.Tasks;
using JetBrains.Annotations;
using Erm.Messaging;

namespace Erm.Messaging.TypedMessageHandler;

[PublicAPI]
public interface IMessageHandler<in TMessage> where TMessage : class
{
    public Task Handle(IReceiveContext context, IEnvelope<TMessage> envelope);
}