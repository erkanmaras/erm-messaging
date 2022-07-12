using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Erm.Messaging.Saga;

//Todo: Process synchronization; memory kullanımı ve performansı test edilmeli veya daha güvenilir bir implementation (battle tested) araştırılmalı.
//source -> https://stackoverflow.com/a/31194647/993434
public sealed class KeyedLocker
{
    private static readonly Dictionary<object, RefCounted<SemaphoreSlim>?> SemaphoreSlims = new();

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private SemaphoreSlim GetOrCreate(object key)
    {
        RefCounted<SemaphoreSlim>? item;
        lock (SemaphoreSlims)
        {
            if (SemaphoreSlims.TryGetValue(key, out item))
            {
                ++item!.RefCount;
            }
            else
            {
                item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
                SemaphoreSlims[key] = item;
            }
        }

        return item.Value;
    }

    public async Task<IDisposable> Lock(object key)
    {
        await GetOrCreate(key).WaitAsync().ConfigureAwait(false);
        return new Releaser(key);
    }

    private sealed class RefCounted<T>
    {
        public RefCounted(T value)
        {
            RefCount = 1;
            Value = value;
        }

        public int RefCount { get; set; }
        public T Value { get; }
    }

    private sealed class Releaser : IDisposable
    {
        public Releaser(object key)
        {
            Key = key;
        }

        private object Key { get; }

        public void Dispose()
        {
            RefCounted<SemaphoreSlim>? item;
            lock (SemaphoreSlims)
            {
                item = SemaphoreSlims[Key];
                --item!.RefCount;
                if (item.RefCount is 0)
                {
                    SemaphoreSlims.Remove(Key);
                }
            }

            item.Value.Release();
        }
    }
}