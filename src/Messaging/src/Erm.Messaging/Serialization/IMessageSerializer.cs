using System;
using System.Threading.Tasks;

namespace Erm.Messaging.Serialization;

public interface IMessageSerializer
{
    //TODO: Research https://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream

    string ContentType { get; }
    Task<byte[]> Serialize(object message);
    Task<object> Deserialize(byte[] value, Type messageType);
}