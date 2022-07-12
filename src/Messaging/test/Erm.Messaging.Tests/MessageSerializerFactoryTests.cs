using System;
using FluentAssertions;
using Moq;
using Erm.Messaging.Serialization;
using Xunit;

namespace Erm.Messaging.Tests;

public class MessageSerializerFactoryTests
{
    [Fact]
    public void WhenContentTypeUnknown_CreateShouldThrowException()
    {
        var factory = new MessageSerializerFactory(new Mock<IServiceProvider>().Object);
        FluentActions.Invoking(() => factory.GetSerializer("invalid-content-type")).Should().Throw<InvalidOperationException>();
    }
}