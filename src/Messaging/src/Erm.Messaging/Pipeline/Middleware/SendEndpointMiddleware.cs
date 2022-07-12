using System.Threading.Tasks;

namespace Erm.Messaging.Pipeline.Middleware;

public class SendEndpointMiddleware : IMessagePipelineMiddleware<ISendContext>
{
    private readonly ISendEndpoint _sendEndpoint;

    public SendEndpointMiddleware(ISendEndpoint sendEndpoint)
    {
        _sendEndpoint = sendEndpoint;
    }

    public async Task Invoke(ISendContext context, IEnvelope envelope, NextDelegate<ISendContext> next)
    {
        await _sendEndpoint.Send(context, envelope);
        await next(context, envelope);
    }
}