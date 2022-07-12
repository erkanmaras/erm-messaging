using System.Threading.Tasks;

namespace Erm.Messaging.Pipeline;

public interface IPipelineExecutor<in TContext> where TContext : IMessageContext
{
    public Task Execute(TContext context, IEnvelope envelope);
}