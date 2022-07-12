using System;
using System.Collections.Generic;

namespace Erm.Messaging.TypedMessageHandler;

public interface IMessageHandlerTypeMap
{
    public IEnumerable<Type> GetMessageHandlerTypes(Type messageType);
}