using System;
using System.Threading.Tasks;

namespace Erm.Messaging.Saga;

internal interface ISagaInitializer
{
    Task<(bool, ISagaStateEntry?)> TryInitialize<TMessage>(ISaga saga, Guid sagaId) where TMessage : class;
}