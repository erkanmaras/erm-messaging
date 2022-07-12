namespace Erm.Messaging.KafkaTransport;

public enum SecurityProtocol
{
    /// <summary>Plaintext</summary>
    Plaintext,

    /// <summary>Ssl</summary>
    Ssl,

    /// <summary>SaslPlaintext</summary>
    SaslPlaintext,

    /// <summary>SaslSsl</summary>
    SaslSsl,
}