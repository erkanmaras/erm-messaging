namespace Erm.Messaging.Saga;

public enum SagaStatus
{
    Pending = 0,
    Completed = 1,
    Rejected = 2
}