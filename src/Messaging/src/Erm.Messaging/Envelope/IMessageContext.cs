using System;

namespace Erm.Messaging;

public interface IMessageContext
{
    ContextProperties ExtendedProperties { get; }
    IServiceProvider ServiceProvider { get; }
}