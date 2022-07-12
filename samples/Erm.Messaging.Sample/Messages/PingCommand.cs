using Erm.Core;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

namespace Erm.Messaging.Sample;

public class PingCommand
{
    public string Version { get; set; } = "1";
    public Guid Id { get; set; } = Uuid.Next();
    public string Data { get; set; } = "Ping";
}