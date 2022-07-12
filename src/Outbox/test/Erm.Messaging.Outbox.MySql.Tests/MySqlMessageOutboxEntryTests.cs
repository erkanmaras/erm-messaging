using System;
using FluentAssertions;
using Moq;
using Erm.Core;
using Xunit;

namespace Erm.Messaging.Outbox.MySql.Tests;

public class MySqlMessageOutboxEntryTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenTheEnvelopeIsNull()
    {
        FluentActions.Invoking(() => new MySqlMessageOutboxEntry(Uuid.Next(), null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTheEntryIdIsEmpty()
    {
        var envelope = new Mock<IMessageEnvelope>().Object;
        FluentActions.Invoking(() => new MySqlMessageOutboxEntry(Guid.Empty, envelope)).Should().Throw<ArgumentException>();
    }
}