using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Erm.KafkaClient;

namespace Erm.Messaging.KafkaTransport;

[PublicAPI]
public class MessagingKafkaContext
{
    public MessagingKafkaContext(IKafkaClient client)
    {
        _client = client;
    }

    private readonly IKafkaClient _client;

    public Task Start(CancellationToken stopCancellationToken = default)
    {
        return _client.Start(stopCancellationToken);
    }

    public Task Stop()
    {
        return _client.Stop();
    }
}