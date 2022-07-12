using System;
using System.Threading.Tasks;
using Erm.Messaging.Pipeline;
using Erm.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging;

public class ReceiveEndpoint : IReceiveEndpoint
{
    private readonly IPipelineExecutor<IReceiveContext> _pipelineExecutor;
    private readonly IMetadataProvider _metadataProvider;
    private readonly IMessageSender _sender;
    private readonly IMessageSerializerFactory _serializerFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ReceiveEndpoint(
        IPipelineExecutor<IReceiveContext> pipelineExecutor,
        IMetadataProvider metadataProvider,
        IMessageSender sender,
        IMessageSerializerFactory serializerFactory,
        IServiceScopeFactory serviceScopeFactory)
    {
        _pipelineExecutor = pipelineExecutor;
        _metadataProvider = metadataProvider;
        _sender = sender;
        _serializerFactory = serializerFactory;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<ReceiveResult> Receive(IMessageEnvelope envelope)
    {
        Validate(envelope);
        var messageName = ResolveMessageType(envelope.MessageName);
        var typedEnvelope = await envelope.ToTyped(messageName, _serializerFactory.GetSerializer(envelope.MessageContentType));
        using var scope = _serviceScopeFactory.CreateScope();
        var context = new ReceiveContext(envelope, _sender, scope.ServiceProvider);
        await _pipelineExecutor.Execute(context, typedEnvelope);
        return context.Result;
    }

    private Type ResolveMessageType(string messageName)
    {
        var messageType = _metadataProvider.GetMessageObjectType(messageName);
        if (messageType is null)
        {
            throw new InvalidOperationException($"Received message-type:{messageType} not found in MetadataProvider!");
        }

        return messageType;
    }

    private static void Validate(IMessageEnvelope envelope)
    {
        if (envelope.Message == null)
        {
            throw new InvalidOperationException("Received envelope message is null!");
        }
    }
}