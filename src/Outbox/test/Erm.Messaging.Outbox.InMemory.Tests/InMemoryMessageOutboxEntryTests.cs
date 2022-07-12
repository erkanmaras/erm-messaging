using System;
using FluentAssertions;
using Moq;
using Erm.Core;
using Xunit;

namespace Erm.Messaging.Outbox.InMemory.Tests;

public class InMemoryMessageOutboxEntryTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenTheEnvelopeIsNull()
    {
        FluentActions.Invoking(() => new InMemoryMessageOutboxEntry(Uuid.Next(), null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTheEntryIdIsEmpty()
    {
        var envelope = new Mock<IMessageEnvelope>().Object;
        FluentActions.Invoking(() => new InMemoryMessageOutboxEntry(Guid.Empty, envelope)).Should().Throw<ArgumentException>();
    }
}