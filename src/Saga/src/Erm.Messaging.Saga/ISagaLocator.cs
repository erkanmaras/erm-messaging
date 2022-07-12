using System;
using System.Collections.Generic;

namespace Erm.Messaging.Saga;

internal interface ISagaLocator
{
    IEnumerable<ISagaAction<TMessage>> Locate<TMessage>(IServiceProvider serviceProvider) where TMessage : class;
}