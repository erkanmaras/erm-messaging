using System;
using JetBrains.Annotations;

namespace Erm.Messaging.Saga;

[PublicAPI]
public abstract class Saga<TData> : Saga, ISaga<TData> where TData : class, new()
{
    public TData Data => (TData)GetData()!;

    public override void Initialize(Guid sagaId, SagaStatus status, object? data = null)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (data is not TData)
        {
            throw new ArgumentException($"{data.GetType()} is not assignable to {typeof(TData)}.");
        }

        base.Initialize(sagaId, status, data);
    }
}