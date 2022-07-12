using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Erm.Messaging.KafkaTransport.TestClient.Shared;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ArgumentsStyleOther

namespace Erm.Messaging.KafkaTransport.IntegrationTests;

public class KafkaTransportSendReceiveTests : IClassFixture<TestClientHostFixture>
{
    private readonly TestClientHostFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public KafkaTransportSendReceiveTests(TestClientHostFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _testOutputHelper = testOutputHelper;
        _testOutputHelper.WriteLine($"{nameof(KafkaTransportSendReceiveTests)}-TestContextUniqueNumber: {_fixture.UniqueKeyForCurrentTestContext}");
    }

    [Fact]
    public async Task KafkaTransport_WhenCommandIsSent_ResponseShouldBeReceived()
    {
        var messageId = await _fixture.SenderApp.SendTurnOnLightsCommand();
        var messageReceived = await CheckForMessageReceived(_fixture.ReceiverApp.TestContext, "TurnOnLights", messageId);
        messageReceived.Should().BeTrue();
        var responseReceived = await CheckForResponseReceived(_fixture.SenderApp.TestContext, "TurnOnLightsResponse", requestId: messageId);
        responseReceived.Should().BeTrue();
    }

    [Fact]
    public async Task KafkaTransport_SendsAndReceivesEvents()
    {
        var eventId = await _fixture.SenderApp.PublishRoomEnlightenedEvent();

        var messageReceived = await CheckForMessageReceived(_fixture.ReceiverApp.TestContext, "RoomEnlightened", eventId);
        messageReceived.Should().BeTrue();
    }

    [Fact]
    public async Task KafkaTransport_WhenQueryIsSent_ResponseShouldBeReceived()
    {
        var messageId = await _fixture.SenderApp.SendAreLightsOnQuery();

        var messageReceived = await CheckForMessageReceived(_fixture.ReceiverApp.TestContext, "AreLightsOn", messageId);
        messageReceived.Should().BeTrue();

        var responseReceived = await CheckForResponseReceived(_fixture.SenderApp.TestContext, "AreLightsOnResponse", requestId: messageId);
        responseReceived.Should().BeTrue();
    }

    private async Task<bool> CheckForMessageReceived(TestContext testContext, string messageName, Guid messageId, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(15);

        const int interval = 200;

        for (var i = 0; i < timeout.Value.TotalMilliseconds / interval; i++)
        {
            await Task.Delay(interval);

            var (_, value) = testContext.Store.FirstOrDefault(kvp => kvp.key == "ReceivedMessage"
                                                                     && ((IEnvelopeHeader)(dynamic)kvp.value).MessageId == messageId
                                                                     && ((object)((dynamic)kvp.value).Message).GetType().Name == messageName);
            if (value is null)
            {
                continue;
            }

            var envelope = value as dynamic;
            var message = (object)envelope.Message;

            _testOutputHelper.WriteLine($"{message.GetType()} with Id:{messageId} received in ~{i * interval} milliseconds!");

            return true;
        }

        return false;
    }

    private async Task<bool> CheckForResponseReceived(TestContext testContext, string messageName, Guid requestId, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(15);

        const int interval = 200;

        for (var i = 0; i < timeout.Value.TotalMilliseconds / interval; i++)
        {
            await Task.Delay(200);

            var (_, value) = testContext.Store.FirstOrDefault(kvp => kvp.key == "ReceivedMessage"
                                                                     && ((object)((dynamic)kvp.value).Message).GetType().Name == messageName
                                                                     && ((IEnvelopeHeader)(dynamic)kvp.value).RequestId == requestId);

            if (value is null)
            {
                continue;
            }

            var envelope = value as dynamic;
            var message = (object)envelope.Message;

            _testOutputHelper.WriteLine($"{message.GetType()} with RequestIdId:{requestId} received in ~{i * interval} milliseconds!");

            return true;
        }

        return false;
    }
}