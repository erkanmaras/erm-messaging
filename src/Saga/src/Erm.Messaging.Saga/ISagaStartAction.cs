namespace Erm.Messaging.Saga;

public interface ISagaStartAction<in TMessage> : ISagaAction<TMessage> where TMessage : class
{
}