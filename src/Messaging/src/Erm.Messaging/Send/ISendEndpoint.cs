using System.Threading.Tasks;

namespace Erm.Messaging;

public interface ISendEndpoint
{
    Task Send(ISendContext context, IEnvelope envelope);
}