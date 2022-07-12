using Google.Protobuf;
using Erm.Messaging;
using Erm.Messaging.Serialization;
using Erm.Serialization.Protobuf;

namespace Erm.Messaging.Serialization.Protobuf;

public class ProtobufMessageSerializer : IMessageSerializer
{
    public const string ContentTypeValue = MessageContentTypes.Protobuf;
    public string ContentType => ContentTypeValue;

    public Task<byte[]> Serialize(object message)
    {
        try
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (message is not IMessage protobufMessage)
            {
                throw new ArgumentException("Message is not an instance of " + typeof(IMessage));
            }

            return Task.FromResult(protobufMessage.ToByteArray());
        }
        catch (Exception ex)
        {
            throw new MessageSerializationException("Message can't be serialized!", ex);
        }
    }

    public Task<object> Deserialize(byte[] value, Type messageType)
    {
        try
        {
            return Task.FromResult(ProtobufSerde.Deserialize(value, messageType));
        }
        catch (Exception ex)
        {
            throw new MessageSerializationException("Message can't be deserialized!", ex);
        }
    }
}