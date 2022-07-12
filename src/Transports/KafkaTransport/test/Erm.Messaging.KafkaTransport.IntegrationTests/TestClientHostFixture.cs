using System;
using System.Threading;
using System.Threading.Tasks;
using Erm.Core;
using Erm.Messaging.KafkaTransport.TestClient.Receiver;
using Erm.Messaging.KafkaTransport.TestClient.Sender;
using Erm.Messaging.TestUtils;

namespace Erm.Messaging.KafkaTransport.IntegrationTests;

// ReSharper disable once ClassNeverInstantiated.Global
public class TestClientHostFixture : IDisposable
{
    public SenderApp SenderApp { get; }
    public ReceiverApp ReceiverApp { get; }
    private readonly CancellationTokenSource _cancellationTokenSource;
    public string UniqueKeyForCurrentTestContext { get; }

    public TestClientHostFixture()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        UniqueKeyForCurrentTestContext = $"test.{Uuid.Next().ToShortString().ToLowerInvariant()}";

        var configuration = ConfigurationHelper.BuildConfiguration();

        SenderApp = new SenderApp(configuration, UniqueKeyForCurrentTestContext);
        ReceiverApp = new ReceiverApp(configuration, UniqueKeyForCurrentTestContext);

        Init();
    }

    private void Init()
    {
        try
        {
            ReceiverApp.RunHost(_cancellationTokenSource.Token);
            SenderApp.RunHost(_cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error occured while starting test apps!", e);
        }
    }

    private async Task StopApps()
    {
        _cancellationTokenSource.Cancel();

        await SenderApp.Stop();
        await ReceiverApp.Stop();
    }

    public void Dispose()
    {
        StopApps().GetAwaiter().GetResult();
    }
}