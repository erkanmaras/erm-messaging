using System;

namespace Erm.Messaging;

public interface IEnvelopeHeader
{
    public Guid MessageId { get; }
    public Guid? RequestId { get; set; }
    public string? ReplyTo { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public Guid? CorrelationId { get; set; }
    public string? GroupId { get; set; }
    public DateTimeOffset? Time { get; set; }

    /// <summary>
    /// The time in seconds.
    /// </summary>
    public int? TimeToLive { get; set; }

    public EnvelopeProperties ExtendedProperties { get; }
}