using System;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class SendContext : ISendContext
{
    public SendContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public SendResult Result { get; } = new();
    public ContextProperties ExtendedProperties { get; } = new();
    public IServiceProvider ServiceProvider { get; }
}