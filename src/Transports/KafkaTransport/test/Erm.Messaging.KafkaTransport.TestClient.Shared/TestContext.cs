using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Erm.Messaging.KafkaTransport.TestClient.Shared;

[PublicAPI]
public class TestContext
{
    public string UniqueKeyForCurrentTestContext { get; }

    public TestContext(string uniqueKeyForCurrentTestContext)
    {
        UniqueKeyForCurrentTestContext = uniqueKeyForCurrentTestContext;
    }

    public readonly ConcurrentBag<(string key, object value)> Store = new();
}