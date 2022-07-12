namespace Erm.Messaging.MessageGateway.MySql;

public delegate string ConnectionStringProvider();

public class MessageStatusRegistryMySqlConfiguration : IMessageStatusRegistryMySqlConfiguration
{
    public MessageStatusRegistryMySqlConfiguration(ConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    private readonly ConnectionStringProvider _connectionStringProvider;

    public string GetConnectionString()
    {
        return _connectionStringProvider();
    }
}