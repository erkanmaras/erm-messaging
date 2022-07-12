using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Erm.Messaging.Pipeline;

[PublicAPI]
public delegate Task NextDelegate<in TContext>(TContext context, IEnvelope envelope);

[PublicAPI]
public interface IMessagePipelineMiddleware<TContext> where TContext : IMessageContext
{
    Task Invoke(TContext context, IEnvelope envelope, NextDelegate<TContext> next);
}