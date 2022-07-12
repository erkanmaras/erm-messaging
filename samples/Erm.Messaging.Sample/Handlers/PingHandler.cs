using JetBrains.Annotations;
using Erm.Messaging.TypedMessageHandler;

namespace Erm.Messaging.Sample;

[PublicAPI]
public class PingHandler : IMessageHandler<PingCommand>
{
    public Task Handle(IReceiveContext context, IEnvelope<PingCommand> envelope)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Ping handled! Ping Power: {0}", envelope.ExtendedProperties["power"]);
        Console.ForegroundColor = color;
        return Task.CompletedTask;
    }
}