using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging.Saga;

internal sealed class SagaLocator : ISagaLocator
{
    public IEnumerable<ISagaAction<TMessage>> Locate<TMessage>(IServiceProvider serviceProvider) where TMessage : class
    {
        var sagaActions = serviceProvider.GetService<IEnumerable<ISagaAction<TMessage>>>() ?? Enumerable.Empty<ISagaAction<TMessage>>();
        var sagaStartActions = serviceProvider.GetService<IEnumerable<ISagaStartAction<TMessage>>>() ?? Enumerable.Empty<ISagaStartAction<TMessage>>();
        return sagaActions
            .Union(sagaStartActions)
            .GroupBy(s => s.GetType())
            .Select(g => g.First())
            .Distinct().ToList();
    }
}