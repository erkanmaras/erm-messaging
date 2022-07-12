using System.Threading.Tasks;
using Erm.MessageOutbox;
using Erm.Messaging;

namespace Erm.Messaging.OutboxTransport;

public class OutboxSendTransport : ISendTransport
{
    public OutboxSendTransport(IMessageOutbox messageOutbox)
    {
        MessageOutbox = messageOutbox;
    }

    private IMessageOutbox MessageOutbox { get; }

    public async Task Send(ISendContext context, IMessageEnvelope envelope)
    {
        await MessageOutbox.Save(envelope).ConfigureAwait(false);
    }
}