using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Erm.Core;
using Erm.Messaging.Serialization.Json;
using Xunit;

namespace Erm.Messaging.Outbox.MySql.Tests;

public class MySqlMessageOutboxTests
{
    [Fact]
    public async Task Save_ShouldSaveTheEntry_WhenTheEntryIsValid()
    {
        var mock = new Mock<IOutboxRepository>();
        mock.Setup(x => x.Save(It.IsAny<MySqlMessageOutboxEntry>()));
        var entry = await CreateEntry();
        var sut = new MySqlMessageOutbox(mock.Object);
        await sut.Save(entry);
        mock.Verify(x => x.Save(It.Is(entry, EqualityComparer<MySqlMessageOutboxEntry>.Default)), Times.Once());
    }

    [Fact]
    public async Task Save_ShouldSaveTheEntry_WhenTheEntryIsValid2()
    {
        var mock = new Mock<IOutboxRepository>();
        mock.Setup(x => x.Save(It.IsAny<MySqlMessageOutboxEntry>()));
        var entry = await CreateEntry();
        var sut = new MySqlMessageOutbox(mock.Object);
        await sut.Save(entry);
        mock.Verify(x => x.Save(It.Is(entry, EqualityComparer<MySqlMessageOutboxEntry>.Default)), Times.Once());
    }

    [Fact]
    public async Task Save_ShouldThrow_WhenTheEntryIsNull()
    {
        var mock = new Mock<IOutboxRepository>();
        var sut = new MySqlMessageOutbox(mock.Object);
        await FluentActions.Awaiting(async () => await sut.Save(null!)).Should().ThrowAsync<ArgumentNullException>();
    }

    private static async Task<MySqlMessageOutboxEntry> CreateEntry(Guid? messageId = null, Guid? entryId = null)
    {
        messageId ??= Uuid.Next();
        entryId ??= Uuid.Next();
        var message = new TestMessage
        {
            Id = messageId.Value
        };
        var serializer = new JsonMessageSerializer();
        var serializedMessage = await serializer.Serialize(message);
        var envelope = new MessageEnvelope(message.Id, message.GetType().FullName!, MessageContentTypes.Json, serializedMessage);
        return new MySqlMessageOutboxEntry(entryId.Value, envelope);
    }

    private class TestMessage
    {
        public Guid Id { get; init; }
    }
}