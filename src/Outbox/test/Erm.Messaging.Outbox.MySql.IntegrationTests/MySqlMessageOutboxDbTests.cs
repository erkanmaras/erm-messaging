using System;
using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;
using FluentAssertions;
using Erm.Core;
using Erm.Messaging.Serialization.Json;
using Xunit;

namespace Erm.Messaging.Outbox.MySql.IntegrationTests;

public class MySqlMessageOutboxDbTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public MySqlMessageOutboxDbTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task Save_InsertsTheMessage()
    {
        // Arrange
        var messageId = Uuid.Next();
        var entry = await CreateEntry(messageId);
        var sut = new MySqlMessageOutbox(_databaseFixture.OutboxRepository);

        // Act
        await sut.Save(entry);

        // Assert
        var foundEntry = await sut.GetEntry(entry.Id);
        foundEntry!.Should().BeEquivalentTo(entry, options => options.Excluding(e => e.CreatedAt));
    }

    [Fact]
    public async Task Save_TheMessageShouldBeRemoved_WhenTransactionScopeWasRolledBack()
    {
        var messageId = Uuid.Next();
        var entry = await CreateEntry(messageId);

        var sut = new MySqlMessageOutbox(_databaseFixture.OutboxRepository);

        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await sut.Save(entry);
            // Rollback
        }

        (await sut.GetEntry(entry.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Save_TheTwoMessageShouldBeRemoved_WhenTransactionScopeWasRolledBack()
    {
        // Arrange
        var messageId1 = Uuid.Next();
        var messageId2 = Uuid.Next();

        var entry1 = await CreateEntry(messageId1);
        var entry2 = await CreateEntry(messageId2);

        var sut = new MySqlMessageOutbox(_databaseFixture.OutboxRepository);

        // Act
        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await sut.Save(entry1);
            await sut.Save(entry2);

            var foundEntry1 = await sut.GetEntry(entry1.Id);
            var foundEntry2 = await sut.GetEntry(entry2.Id);

            foundEntry1!.MessageId.Should().Be(messageId1.ToString());
            foundEntry2!.MessageId.Should().Be(messageId2.ToString());

            // Rollback
        }

        (await sut.GetEntry(messageId1)).Should().BeNull();
        (await sut.GetEntry(messageId2)).Should().BeNull();
    }

    [Fact]
    public async Task Save_TheMessageShouldNotBeRemoved_WhenTransactionScopeWasCompleted()
    {
        // Arrange
        var messageId = Uuid.Next();
        var entry = await CreateEntry(messageId);

        var sut = new MySqlMessageOutbox(_databaseFixture.OutboxRepository);

        // Act
        using (var trans = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await sut.Save(entry);
            trans.Complete();
        }

        // Assert
        var foundEntry = await sut.GetEntry(entry.Id);
        foundEntry!.MessageId.Should().Be(messageId.ToString());
        foundEntry!.Message.Should().NotBeNull();
    }

    [Fact]
    public async Task Save_TheTwoMessageShouldNotBeRemoved_WhenTransactionScopeWasCompleted()
    {
        var messageId1 = Uuid.Next();
        var messageId2 = Uuid.Next();
        var entry1 = await CreateEntry(messageId1);
        var entry2 = await CreateEntry(messageId2);

        var sut = new MySqlMessageOutbox(_databaseFixture.OutboxRepository);

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await sut.Save(entry1);
            await sut.Save(entry2);
            scope.Complete();
        }

        var foundEntry1 = await sut.GetEntry(entry1.Id);
        var foundEntry2 = await sut.GetEntry(entry2.Id);

        foundEntry1!.MessageId.Should().Be(messageId1.ToString());
        foundEntry2!.MessageId.Should().Be(messageId2.ToString());
    }

    [Fact]
    public async Task Save_ShouldThrow_WhenDuplicateMessageIdIsAdded()
    {
        // Arrange
        var messageId = Uuid.Next();

        var entry = await CreateEntry(messageId);

        var sut = new MySqlMessageOutbox(_databaseFixture.OutboxRepository);

        await sut.Save(entry);

        // Act
        await FluentActions.Awaiting(() => sut.Save(entry))
            .Should().ThrowAsync<DbException>().WithMessage("*duplicate*"); // Assert
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
        var envelope = new MessageEnvelope(message.Id, message.GetType().FullName!, MessageContentTypes.Json, serializedMessage)
        {
            // ReSharper disable StringLiteralTypo
            Destination = @"L'ombelico del mondo"
            // ReSharper restore StringLiteralTypo
        };
        return new MySqlMessageOutboxEntry(entryId.Value, envelope);
    }

    private class TestMessage
    {
        public Guid Id { get; init; }
    }
}