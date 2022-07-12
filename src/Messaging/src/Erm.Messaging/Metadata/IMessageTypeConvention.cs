using System;
using JetBrains.Annotations;

namespace Erm.Messaging;

public interface IMessageTypeConvention
{
    public bool IsEvent(Type type);
    public bool IsCommand(Type type);
    public bool IsCommandResponse(Type type);
    public bool IsQuery(Type type);
    public bool IsQueryResponse(Type type);
}

// Era platform default message conventions
[PublicAPI]
public class MessageTypeConvention : IMessageTypeConvention
{
    public virtual bool IsEvent(Type type)
    {
        return type.Namespace != null &&
               type.Namespace.StartsWith("Erm.") &&
               type.Namespace!.EndsWith("Messages.Events");
    }

    public virtual bool IsCommand(Type type)
    {
        return type.Namespace != null &&
               type.Namespace!.StartsWith("Erm.") &&
               type.Namespace!.EndsWith("Messages.Commands") &&
               !type.Name.EndsWith("Response");
    }

    public virtual bool IsCommandResponse(Type type)
    {
        return type.Namespace != null &&
               type.Namespace!.StartsWith("Erm.") &&
               type.Namespace!.EndsWith("Messages.Commands") &&
               type.Name.EndsWith("Response");
    }

    public virtual bool IsQuery(Type type)
    {
        return type.Namespace != null &&
               type.Namespace!.StartsWith("Erm.") &&
               type.Namespace!.EndsWith("Messages.Queries") &&
               !type.Name.EndsWith("Response");
    }

    public virtual bool IsQueryResponse(Type type)
    {
        return type.Namespace != null &&
               type.Namespace!.StartsWith("Erm.") &&
               type.Namespace!.EndsWith("Messages.Queries") &&
               type.Name.EndsWith("Response");
    }
}