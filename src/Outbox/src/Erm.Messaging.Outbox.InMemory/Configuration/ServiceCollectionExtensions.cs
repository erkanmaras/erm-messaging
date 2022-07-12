using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Erm.Core;
using Erm.MessageOutbox;

namespace Erm.Messaging.Outbox.InMemory;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryMessageOutbox(this IServiceCollection services)
    {
        return services.AddSingleton<IMessageOutbox>(x => new InMemoryMessageOutbox(x.GetRequiredService<IClock>()));
    }
}