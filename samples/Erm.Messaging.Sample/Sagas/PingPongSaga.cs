using JetBrains.Annotations;
using Erm.Messaging.Saga;

namespace Erm.Messaging.Sample;

[PublicAPI]
public class PingPongSaga : Saga<SagaData>, ISagaStartAction<PingCommand>, ISagaAction<PongCommand>
{
    private readonly IMessageSender _sender;

    protected override Guid ResolveId<TMessage>(IReceiveContext context, IEnvelope<TMessage> envelope)
    {
        // for simplicity
        return Guid.Parse("42263e34-17be-4032-91bf-73205af4c9e1");
    }

    public PingPongSaga(IMessageSender sender)
    {
        _sender = sender;
    }

    public async Task Handle(IReceiveContext context, IEnvelope<PingCommand> envelope)
    {
        Data.PingCount += 1;
        await Task.Delay(1000);
        TryCompleteSaga();
        if (Status == SagaStatus.Pending)
        {
            await _sender.Send(new PongCommand());
        }
    }

    public async Task Handle(IReceiveContext context, IEnvelope<PongCommand> envelope)
    {
        Data.PongCount += 1;
        await Task.Delay(1000);
        TryCompleteSaga();
        if (Status == SagaStatus.Pending)
        {
            await _sender.Send(new PingCommand());
        }
    }

    public Task Compensate(IReceiveContext context, IEnvelope<PingCommand> envelope)
    {
        return Task.CompletedTask;
    }

    public Task Compensate(IReceiveContext context, IEnvelope<PongCommand> envelope)
    {
        return Task.CompletedTask;
    }

    private void TryCompleteSaga()
    {
        if (Data.PingCount <= 10 || Data.PongCount <= 10)
        {
            return;
        }

        Complete();
        Console.BackgroundColor = ConsoleColor.Green;
        Console.WriteLine("Game Completed!");
    }
}

public class SagaData
{
    public int PingCount { get; set; }
    public int PongCount { get; set; }
}