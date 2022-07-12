using System;
using System.Threading.Tasks;
using System.Transactions;
using FluentAssertions;
using Erm.Core;
using Erm.Messaging.Serialization.Json;
using Xunit;

namespace Erm.Messaging.Outbox.InMemory.Tests;

public class InMemoryMessageOutboxTests
{
    [Fact]
    public async Task Save_ShouldAddTheEntryIntoInternalCollection_WhenTheEntryIsValid()
    {
        var entry = await CreateEntry();
        var sut = new InMemoryMessageOutbox(new Clock());
        await sut.Save(entry);

        var getEntryResult = await sut.GetEntry(entry.Id);
        getEntryResult.Should().BeEquivalentTo(entry);
    }

    [Fact]
    public async Task Save_ShouldAddTheEntry_AndStatusPropertyShouldBePending()
    {
        var entry = await CreateEntry();
        var sut = new InMemoryMessageOutbox(new Clock());
        await sut.Save(entry);

        var getEntryResult = await sut.GetEntry(entry.Id);
        getEntryResult.Should().BeEquivalentTo(entry);
    }

    [Fact]
    public async Task Save_ShouldAddTheEntryIntoInternalCollection_WhenTheEntryIsValid2()
    {
        var entry = await CreateEntry();
        var sut = new InMemoryMessageOutbox(new Clock());
        await sut.Save(entry);

        var getEntryResult = await sut.GetEntry(entry.Id);
        getEntryResult.Should().BeEquivalentTo(entry);
    }

    [Fact]
    public async Task Save_ShouldThrow_WhenTheEntryIsNull()
    {
        var sut = new InMemoryMessageOutbox(new Clock());
        await FluentActions.Awaiting(async () => await sut.Save(null!)).Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Save_TheMessageShouldBeRemoved_WhenTransactionScopeWasRolledBack()
    {
        var entry = await CreateEntry();
        var sut = new InMemoryMessageOutbox(new Clock());

        using (new TransactionScope())
        {
            await sut.Save(entry);
            var getEntryResult = await sut.GetEntry(entry.Id);
            getEntryResult.Should().BeEquivalentTo(entry);
        }

        var getEntryResultNull = await sut.GetEntry(entry.Id);
        getEntryResultNull.Should().BeNull();
    }

    [Fact]
    public async Task Save_TheTwoMessageShouldBeRemoved_WhenTransactionScopeWasRolledBack()
    {
        var entry1 = await CreateEntry();
        var entry2 = await CreateEntry();
        var sut = new InMemoryMessageOutbox(new Clock());

        using (new TransactionScope())
        {
            await sut.Save(entry1);
            await sut.Save(entry2);

            var getEntryResult1 = await sut.GetEntry(entry1.Id);
            var getEntryResult2 = await sut.GetEntry(entry2.Id);

            getEntryResult1.Should().BeEquivalentTo(entry1);
            getEntryResult2.Should().BeEquivalentTo(entry2);
        }

        (await sut.GetEntry(entry1.Id)).Should().BeNull();
        (await sut.GetEntry(entry2.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Save_TheMessageShouldNotBeRemoved_WhenTransactionScopeWasCompleted()
    {
        var entry = await CreateEntry();
        var sut = new InMemoryMessageOutbox(new Clock());

        using (var scope = new TransactionScope())
        {
            await sut.Save(entry);
            scope.Complete();
        }

        var getEntryResult = await sut.GetEntry(entry.Id);
        getEntryResult.Should().BeEquivalentTo(entry);
    }

    [Fact]
    public async Task Save_TheTwoMessageShouldNotBeRemoved_WhenTransactionScopeWasCompleted()
    {
        var entry1 = await CreateEntry();
        var entry2 = await CreateEntry();
        var sut = new InMemoryMessageOutbox(new Clock());

        using (var scope = new TransactionScope())
        {
            await sut.Save(entry1);
            await sut.Save(entry2);
            scope.Complete();
        }

        var getEntryResult1 = await sut.GetEntry(entry1.Id);
        var getEntryResult2 = await sut.GetEntry(entry2.Id);

        getEntryResult1.Should().BeEquivalentTo(entry1);
        getEntryResult2.Should().BeEquivalentTo(entry2);
    }

    [Fact]
    public async Task Save_DoesNoAction_WhenTheSameEntryAdded()
    {
        var entry = await CreateEntry();
        var sut = new InMemoryMessageOutbox(new Clock());

        await sut.Save(entry);
        await sut.Save(entry);

        var getEntryResult = await sut.GetEntry(entry.Id);
        getEntryResult.Should().BeEquivalentTo(entry);
    }

    [Fact]
    public async Task Save_ShouldThrow_WhenDifferentEntryAddedWithAnExistingEntryId()
    {
        var sut = new InMemoryMessageOutbox(new Clock());

        var entryId = Uuid.Next();
        var entry = await CreateEntry(entryId: entryId);
        await sut.Save(entry);

        // Add different entry with the same entry id.
        await FluentActions.Awaiting(async () => await sut.Save(await CreateEntry(entryId: entryId))).Should().ThrowAsync<InvalidOperationException>();
    }

    private static async Task<InMemoryMessageOutboxEntry> CreateEntry(Guid? messageId = null, Guid? entryId = null)
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
        return new InMemoryMessageOutboxEntry(entryId.Value, envelope);
    }

    private class TestMessage
    {
        public Guid Id { get; init; }
    }
}