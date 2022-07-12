using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using FluentAssertions;
using Moq;
using Erm.Core;
using Xunit;

namespace Erm.Messaging.MessageGateway.MySql.IntegrationTests;

public class MySqlMessageStatusRegistryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public MySqlMessageStatusRegistryTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Theory]
    [InlineData(MessageStatus.Processing)]
    [InlineData(MessageStatus.Succeeded)]
    [InlineData(MessageStatus.Faulted)]
    public async Task MarkAsStatus_ShouldAddCorrectEntry(MessageStatus status)
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();

        var statusEntry = status switch
        {
            MessageStatus.Processing => await registry.MarkAsProcessing(messageId),
            MessageStatus.Succeeded => await registry.MarkAsSucceeded(messageId),
            MessageStatus.Faulted => await registry.MarkAsFaulted(messageId, new MessageFaultDetails("TestFaultType", "TestFaultDetails")),
            _ => throw new ArgumentOutOfRangeException(nameof(MessageStatus), status, "Unhandled MessageStatus type!")
        };

        statusEntry.MessageId.Should().Be(messageId);
        statusEntry.MessageStatus.Should().Be(status);

        var lastEntry = await registry.GetLastEntry(messageId);
        lastEntry.Should().NotBeNull();
        lastEntry!.MessageId.Should().Be(statusEntry.MessageId);
        lastEntry.MessageStatus.Should().Be(statusEntry.MessageStatus);
    }

    [Fact]
    public async Task MarkAsProcessing_ShouldThrowDuplicateException_WhenLastStateIsProcessing()
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();
        await registry.MarkAsProcessing(messageId);
        await registry.Invoking(async r => await r.MarkAsProcessing(messageId)).Should().ThrowAsync<DuplicateMessageProcessingException>();
    }

    [Fact]
    public async Task MarkAsSucceeded_ShouldNotThrowDuplicateException_WhenLastStateIsProcessing()
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();
        await registry.MarkAsProcessing(messageId);
        await registry.Invoking(async r => await r.MarkAsSucceeded(messageId)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task MarkAsProcessing_ShouldThrowDuplicateException_WhenLastStateIsSucceeded()
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();
        await registry.MarkAsProcessing(messageId);
        await registry.MarkAsSucceeded(messageId);
        await registry.Invoking(async r => await r.MarkAsProcessing(messageId)).Should().ThrowAsync<DuplicateMessageProcessingException>();
    }

    [Fact]
    public async Task MarkAsProcessing_ShouldNotThrowException_WhenLastStateIsFaulted()
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();
        await registry.MarkAsProcessing(messageId);
        await registry.MarkAsFaulted(messageId, new MessageFaultDetails("FaultType", "FaultMessage"));
        await registry.Invoking(async r => await r.MarkAsProcessing(messageId)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetEntries_ShouldReturnEntries_WithCorrectOrder()
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();
        var faultDetails = new MessageFaultDetails("ExType1", "ExMessage1");
        await registry.MarkAsProcessing(messageId);
        await registry.MarkAsFaulted(messageId, faultDetails);
        await registry.MarkAsProcessing(messageId);
        await registry.MarkAsSucceeded(messageId);
        var allEntries = (await registry.GetEntries(messageId)).ToList();

        allEntries[0].MessageStatus.Should().Be(MessageStatus.Processing);
        allEntries[1].MessageStatus.Should().Be(MessageStatus.Faulted);
        allEntries[2].MessageStatus.Should().Be(MessageStatus.Processing);
        allEntries[3].MessageStatus.Should().Be(MessageStatus.Succeeded);
    }

    [Fact]
    public async Task GetLastEntry_ShouldReturn_CorrectEntry()
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();

        await registry.MarkAsProcessing(messageId);
        await registry.MarkAsFaulted(messageId, new MessageFaultDetails("FaultType", "FaultMessage"));
        await registry.MarkAsProcessing(messageId);
        await registry.MarkAsSucceeded(messageId);
        var lastEntry = await registry.GetLastEntry(messageId);

        lastEntry.Should().NotBeNull();
        lastEntry!.MessageId.Should().Be(messageId);
        lastEntry.MessageStatus.Should().Be(MessageStatus.Succeeded);
    }

    [Fact]
    public async Task MarkAsSucceeded_ShouldAddCorrectEntry()
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();

        await registry.MarkAsSucceeded(messageId);
        var lastEntry = await registry.GetLastEntry(messageId);

        lastEntry.Should().NotBeNull();
        lastEntry!.MessageId.Should().Be(messageId);
        lastEntry.MessageStatus.Should().Be(MessageStatus.Succeeded);
    }

    [Fact]
    public async Task MarkAsFaulted_ShouldAddCorrectEntry()
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();

        var faultDetails1 = new MessageFaultDetails("ExType1", "ExMessage1");
        await registry.MarkAsFaulted(messageId, faultDetails1);

        var lastEntry = await registry.GetLastEntry(messageId);
        lastEntry.Should().NotBeNull();
        lastEntry!.MessageId.Should().Be(messageId);
        lastEntry.MessageStatus.Should().Be(MessageStatus.Faulted);
        lastEntry.FaultDetails.Should().BeEquivalentTo(faultDetails1);
    }

    [Fact]
    public async Task MarkAsProcessing_ShouldBeThreadSafe()
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();
        const int taskCount = 10;
        var taskList = new List<Task>(taskCount);
        var exceptionCount = 0;
        for (var i = 0; i < taskCount; i++)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await registry.MarkAsProcessing(messageId);
                }
                catch (DuplicateMessageProcessingException)
                {
                    Interlocked.Increment(ref exceptionCount);
                }
            });
            taskList.Add(task);
        }

        Task.WaitAll(taskList.ToArray());
        var entries = await registry.GetEntries(messageId);
        entries.Count().Should().Be(1);
        exceptionCount.Should().Be(taskCount - 1);
    }

    [Fact]
    public async Task EnlistInCurrent_TransactionScope_HandleRollback()
    {
        var registry = CreateMySqlMessageStatusRegistry();
        var messageId = Uuid.Next();
        await registry.MarkAsProcessing(messageId);
        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await registry.MarkAsFaulted(messageId, new MessageFaultDetails("", ""));
            await registry.MarkAsSucceeded(messageId);
            //dont complete 
        }

        var lastEntry = await registry.GetLastEntry(messageId);
        lastEntry?.MessageId.Should().Be(messageId);
        lastEntry?.MessageStatus.Should().Be(MessageStatus.Processing);
        var entries = await registry.GetEntries(messageId);
        entries.Count().Should().Be(1);
    }

    [Fact]
    public async Task EntryDates_ShouldSetFrom_IClock()
    {
        var clock = new Mock<IClock>();
        var expectedDateTime = new DateTimeOffset(1920, 4, 23, 12, 0, 0, TimeSpan.Zero);
        clock.Setup(o => o.UtcNow).Returns(expectedDateTime);

        var registry = new MySqlMessageStatusRegistry(
            new MessageStatusRegistryMySqlConfiguration(() => _databaseFixture.ConnectionString), clock.Object);
        var messageId = Uuid.Next();
        await registry.MarkAsProcessing(messageId);

        var lastEntry = await registry.GetLastEntry(messageId);
        lastEntry?.CreatedAt.Should().Be(expectedDateTime);
    }

    private MySqlMessageStatusRegistry CreateMySqlMessageStatusRegistry()
    {
        var clock = new Mock<IClock>();
        clock.Setup(o => o.UtcNow).Returns(DateTimeOffset.UtcNow);
        return new MySqlMessageStatusRegistry(
            new MessageStatusRegistryMySqlConfiguration(() => _databaseFixture.ConnectionString),
            clock.Object);
    }
}