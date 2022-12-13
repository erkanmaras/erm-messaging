using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Erm.Messaging;
using AsyncKeyedLock;

namespace Erm.Messaging.Saga;

internal sealed class SagaCoordinator : ISagaCoordinator
{
    private readonly ISagaLocator _locator;
    private readonly ISagaInitializer _initializer;
    private readonly ISagaProcessor _processor;
    private readonly ILogger<SagaCoordinator> _logger;
    private static readonly AsyncKeyedLocker<Guid> Locker = new();

    public SagaCoordinator(ISagaLocator locator,
        ISagaInitializer initializer,
        ISagaProcessor processor,
        ILogger<SagaCoordinator> logger)
    {
        _locator = locator;
        _initializer = initializer;
        _processor = processor;
        _logger = logger;
    }


    public async Task Process<TMessage>(
        IReceiveContext context,
        IEnvelope<TMessage> envelope,
        Func<IReceiveContext, IEnvelope<TMessage>, Task>? onCompleted = null,
        Func<IReceiveContext, IEnvelope<TMessage>, Task>? onRejected = null) where TMessage : class
    {
        //TODO:Saga locate işlemi her message için(bir saga action olmasa bile) çalışacak, nerelerde optimizasyon yapılabilir incelenmeli.?
        var actions = _locator.Locate<TMessage>(context.ServiceProvider).ToList();
        if (actions.Count == 0) //No saga defined for this message.
        {
            _logger.NoSagaFound();
            return;
        }

        var sagaTasks = actions
            .Select(action => Process(context, envelope, action, onCompleted, onRejected))
            .ToList();

        await Task.WhenAll(sagaTasks).ConfigureAwait(false);
    }

    private async Task Process<TMessage>(
        IReceiveContext context,
        IEnvelope<TMessage> envelope,
        ISagaAction<TMessage> action,
        Func<IReceiveContext, IEnvelope<TMessage>, Task>? onCompleted,
        Func<IReceiveContext, IEnvelope<TMessage>, Task>? onRejected) where TMessage : class
    {
        var saga = (ISaga)action;
        var sagaId = saga.GetSagaId(context, envelope, action is ISagaStartAction<TMessage>);
        using (_logger.BeginScope("Saga {SagaType}[{SagaId}]", saga.GetType().FullName, sagaId))
        {
            using (await Locker.LockAsync(sagaId).ConfigureAwait(false))
            {
                var (initialized, state) = await _initializer.TryInitialize<TMessage>(saga, sagaId).ConfigureAwait(false);
                if (!initialized)
                {
                    return;
                }

                try
                {
                    await _processor.Process(saga, context, envelope, state!).ConfigureAwait(false);
                }
                finally
                {
                    await _processor.PostProcess(saga, context, envelope, onCompleted, onRejected).ConfigureAwait(false);
                }
            }
        }
    }
}