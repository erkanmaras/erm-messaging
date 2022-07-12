namespace Erm.Messaging.Serialization;

public interface IMessageSerializerFactory
{
    public IMessageSerializer GetSerializer(string contentType);
}