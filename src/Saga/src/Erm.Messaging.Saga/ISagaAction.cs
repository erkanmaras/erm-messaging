using System.Threading.Tasks;
using Erm.Messaging;

namespace Erm.Messaging.Saga;

public interface ISagaAction<in TMessage> where TMessage : class
{
    Task Handle(IReceiveContext context, IEnvelope<TMessage> envelope);
    Task Compensate(IReceiveContext context, IEnvelope<TMessage> envelope);
}