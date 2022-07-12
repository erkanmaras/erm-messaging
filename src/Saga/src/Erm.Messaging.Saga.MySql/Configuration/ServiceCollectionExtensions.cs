using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Erm.Messaging.Saga;

namespace Erm.Messaging.Saga.MySql;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static ISagaConfiguration UseMySqlPersistence(this ISagaConfiguration configuration, Func<string> connectionStringProvider)
    {
        configuration.Services.AddTransient<ConnectionStringProvider>(_ => connectionStringProvider.Invoke);
        configuration.Services.AddTransient<IMySqlSagaConfiguration, MySqlSagaConfiguration>();
        configuration.Services.AddTransient<ISagaRepository, MySqlSagaRepository>();
        return configuration;
    }
}