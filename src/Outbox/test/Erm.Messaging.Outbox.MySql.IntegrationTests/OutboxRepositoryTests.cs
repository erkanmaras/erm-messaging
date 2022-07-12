using System;
using System.Threading.Tasks;
using FluentAssertions;
using Erm.Core;
using Erm.Messaging.Serialization.Json;
using Xunit;

namespace Erm.Messaging.Outbox.MySql.IntegrationTests;

public class OutboxRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public OutboxRepositoryTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task GetById_ShouldGetTheMessage_WhenMessageExists()
    {
        // Arrange
        var messageId = Uuid.Next();
        var sut = CreateOutboxRepository();
        var entry = await GetValidEntry(messageId);
        await sut.Save(entry);

        // Act
        var selectedEntry = await sut.GetById(entry.Id);

        // Assert
        selectedEntry.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenEntryNotExist()
    {
        // Arrange
        var nonExistingId = Uuid.Next();
        var sut = CreateOutboxRepository();
        (await sut.GetById(nonExistingId)).Should().BeNull();
    }

    private OutboxRepository CreateOutboxRepository()
    {
        // Create new for every test method instead of using single instance at _databaseFixture.OutboxRepository
        try
        {
            return new OutboxRepository(new Clock(), _databaseFixture.ConnectionString);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Cannot connect to the test database!", e);
        }
    }

    private static async Task<MySqlMessageOutboxEntry> GetValidEntry(Guid messageId)
    {
        var message = new TestMessage
        {
            Id = messageId
        };

        var serializer = new JsonMessageSerializer();
        var serializedMessage = await serializer.Serialize(message);
        var envelope = new MessageEnvelope(message.Id, message.GetType().FullName!, MessageContentTypes.Json, serializedMessage);
        return new MySqlMessageOutboxEntry(Uuid.Next(), envelope) { Destination = "Coco Jamboo" };
    }

    private class TestMessage
    {
        public Guid Id { get; init; }
    }
}