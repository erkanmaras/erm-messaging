namespace Erm.Messaging;

public interface IEnvelope<out TMessage> : IEnvelope where TMessage : class
{
    public new TMessage Message { get; }
}

public interface IEnvelope : IEnvelopeHeader
{
    public object Message { get; }
    public string MessageName { get; }
}