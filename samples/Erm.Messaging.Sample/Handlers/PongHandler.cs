using JetBrains.Annotations;
using Erm.Messaging.TypedMessageHandler;

namespace Erm.Messaging.Sample;

[PublicAPI]
public class PongHandler : IMessageHandler<PongCommand>
{
    public Task Handle(IReceiveContext context, IEnvelope<PongCommand> envelope)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Pong handled! Pong Power: {0} - Stamp: {1}", envelope.ExtendedProperties["power"], envelope.ExtendedProperties["stamp"]);
        Console.ForegroundColor = color;
        return Task.CompletedTask;
    }
}