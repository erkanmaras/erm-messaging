using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Erm.Core;
using Erm.MessageOutbox;
using Xunit;

namespace Erm.Messaging.OutboxTransport.Tests;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public class MessageSenderOutboxTransportTests
{
    [Fact]
    public async Task OutboxMessageSender_Should_SendMessage()
    {
        var testContext = new List<IMessageOutboxEntry>();
        var outbox = new FakeMessageOutbox(testContext);
        var sut = new OutboxSendTransport(outbox);
        var envelope = new MessageEnvelope(Uuid.Next(), "Name", "Type", null!);
        await sut.Send(new SendContext(Mock.Of<IServiceProvider>()), envelope);
        testContext.Should().HaveCount(1);
        testContext.First().MessageId.Should().Be(envelope.MessageId.ToString());
    }

    private class FakeMessageOutbox : IMessageOutbox
    {
        private readonly List<IMessageOutboxEntry> _testContext;

        public FakeMessageOutbox(List<IMessageOutboxEntry> testContext)
        {
            _testContext = testContext;
        }

        public Task<IMessageOutboxEntry?> GetEntry(Guid entryId)
        {
            return Task.FromResult<IMessageOutboxEntry?>(null);
        }

        public Task Save(IMessageOutboxEntry outboxEntry)
        {
            _testContext.Add(outboxEntry);
            return Task.CompletedTask;
        }

        public IMessageOutboxEntry CreateEntry(IMessageEnvelope envelope)
        {
            var mock = new Mock<IMessageOutboxEntry>();
            mock.Setup(m => m.MessageId).Returns(() => envelope.MessageId);
            return mock.Object;
        }
    }
}