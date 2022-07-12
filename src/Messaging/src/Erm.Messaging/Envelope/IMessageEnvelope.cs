using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public interface IMessageEnvelope : IEnvelopeHeader
{
    string MessageName { get; set; }
    string MessageContentType { get; set; }
    byte[] Message { get; set; }
}