using System.Threading.Tasks;

namespace Erm.Messaging.Pipeline;

public interface IMessagePipeline<in TContext> where TContext : IMessageContext
{
    Task Invoke(TContext context, IEnvelope envelope);
}