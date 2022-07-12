namespace Erm.Messaging.Saga.MySql;

public delegate string ConnectionStringProvider();

public class MySqlSagaConfiguration : IMySqlSagaConfiguration
{
    public MySqlSagaConfiguration(ConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    private readonly ConnectionStringProvider _connectionStringProvider;

    public string GetConnectionString()
    {
        return _connectionStringProvider();
    }
}

public interface IMySqlSagaConfiguration
{
    public string GetConnectionString();
}