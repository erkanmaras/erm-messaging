using System.Threading.Tasks;

namespace Erm.Messaging;

public interface IReceiveEndpoint
{
    Task<ReceiveResult> Receive(IMessageEnvelope envelope);
}