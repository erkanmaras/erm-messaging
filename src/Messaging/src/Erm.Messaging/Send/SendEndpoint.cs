using System;
using System.Threading.Tasks;
using Erm.Messaging.Serialization;
using JetBrains.Annotations;

namespace Erm.Messaging;

[UsedImplicitly]
public class SendEndpoint : ISendEndpoint
{
    private readonly ISendTransport _transport;
    private readonly IMessageSerializerFactory _messageSerializerFactory;
    private readonly IMetadataProvider _metadataProvider;

    public SendEndpoint(ISendTransport transport, IMessageSerializerFactory messageSerializerFactory, IMetadataProvider metadataProvider)
    {
        _transport = transport;
        _messageSerializerFactory = messageSerializerFactory;
        _metadataProvider = metadataProvider;
    }


    public async Task Send(ISendContext context, IEnvelope envelope)
    {
        var contentType = GetContentType(envelope);
        var messageEnvelope = await envelope.ToEncoded(_messageSerializerFactory.GetSerializer(contentType));
        await _transport.Send(context, messageEnvelope);
    }

    private string GetContentType(IEnvelope envelope)
    {
        //if custom content-type exist in properties, use it
        var contentType = envelope.ExtendedProperties.GetContentType();

        if (string.IsNullOrWhiteSpace(contentType))
        {
            contentType = _metadataProvider.GetContentType(envelope.MessageName);
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new InvalidOperationException($"ContentType not specified for {envelope.Message.GetType()}!");
        }

        return contentType;
    }
}