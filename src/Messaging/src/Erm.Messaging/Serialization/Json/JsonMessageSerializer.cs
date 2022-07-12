using System;
using System.IO;
using System.Threading.Tasks;
using Erm.Serialization.Json;

namespace Erm.Messaging.Serialization.Json;

public class JsonMessageSerializer : IMessageSerializer
{
    public string ContentType => MessageContentTypes.Json;

    public Task<byte[]> Serialize(object message)
    {
        try
        {
            return Task.FromResult(JsonSerde.SerializeToUtf8Bytes(message, message.GetType()));
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
            using (var stream = new MemoryStream(value))
            {
                var message = JsonSerde.Deserialize(stream, messageType);
                return Task.FromResult(message!);
            }
        }
        catch (Exception ex)
        {
            throw new MessageSerializationException("Message can't be deserialized!", ex);
        }
    }
}