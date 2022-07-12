using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MySqlConnector;
using Erm.Core;
using Erm.DbMigration.MySql;
using Erm.Messaging.TestUtils;

namespace Erm.Messaging.Outbox.MySql.IntegrationTests;

// ReSharper disable once ClassNeverInstantiated.Global
public class DatabaseFixture : IDisposable
{
    private readonly IConfiguration _configuration;

    public DatabaseFixture()
    {
        _configuration = BuildConfiguration();
        MySqlMigrator.EnsureDatabaseCreated(ConnectionString, NullLogger.Instance).GetAwaiter().GetResult();
        MySqlMigrator.RunSqlFiles(typeof(MySqlMessageOutbox).Assembly, ConnectionString, NullLogger.Instance).GetAwaiter().GetResult();
    }

    private OutboxRepository _outboxRepository;
    internal OutboxRepository OutboxRepository => _outboxRepository ??= CreateNewOutboxRepository();

    public string ConnectionString => CreateConnectionString();


    private IConfiguration BuildConfiguration()
    {
        return ConfigurationHelper.BuildConfiguration(userSecretsAssembly: GetType().Assembly);
    }

    private string CreateConnectionString()
    {
        var section = _configuration.GetSection("MySql");
        var builder = new MySqlConnectionStringBuilder(section["ConnectionString"])
        {
            GuidFormat = MySqlGuidFormat.Binary16
        };
        return builder.ConnectionString;
    }


    private OutboxRepository CreateNewOutboxRepository()
    {
        return new OutboxRepository(new Clock(), ConnectionString);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}