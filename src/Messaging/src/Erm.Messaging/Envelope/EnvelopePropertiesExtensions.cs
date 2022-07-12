namespace Erm.Messaging;

public static class EnvelopePropertiesExtensions
{
    public static string? GetContentType(this EnvelopeProperties properties)
    {
        return properties.Get(WellKnownEnvelopeProperties.ContentType);
    }

    public static void SetContentType(this EnvelopeProperties properties, string contentType)
    {
        properties.Set(WellKnownEnvelopeProperties.ContentType, contentType);
    }
}

public static class WellKnownEnvelopeProperties
{
    public const string ContentType = "ContentType";
}