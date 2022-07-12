using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Erm.Core;
using Erm.MessageOutbox;

namespace Erm.Messaging.Outbox.MySql;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMySqlMessageOutbox(this IServiceCollection services, Func<string> connectionStringProvider)
    {
        return services.AddSingleton<IMessageOutbox, MySqlMessageOutbox>()
            .AddSingleton<IOutboxRepository>(x => new OutboxRepository(x.GetRequiredService<IClock>(), connectionStringProvider()));
    }
}