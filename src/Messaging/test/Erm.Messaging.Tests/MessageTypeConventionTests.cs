using FluentAssertions;
using Xunit;

// ReSharper disable RedundantNameQualifier

namespace Erm.Messaging.Tests
{
    public class MessageTypeConventionTests
    {
        [Fact]
        public void Should_Define_Correct_MessageTypes()
        {
            var convention = new MessageTypeConvention();

            //type valid and correct function 
            convention.IsEvent(typeof(Erm.Valid.Messages.Events.SomethingHappened)).Should().BeTrue();
            convention.IsCommand(typeof(Erm.Valid.Messages.Commands.DoSomething)).Should().BeTrue();
            convention.IsCommandResponse(typeof(Erm.Valid.Messages.Commands.DoSomethingResponse)).Should().BeTrue();
            convention.IsQuery(typeof(Erm.Valid.Messages.Queries.GetSomething)).Should().BeTrue();
            convention.IsQueryResponse(typeof(Erm.Valid.Messages.Queries.GetSomethingResponse)).Should().BeTrue();

            //type valid but wrong function
            convention.IsEvent(typeof(Erm.Valid.Messages.Commands.DoSomething)).Should().BeFalse();
            convention.IsCommand(typeof(Erm.Valid.Messages.Commands.DoSomethingResponse)).Should().BeFalse();
            convention.IsCommandResponse(typeof(Erm.Valid.Messages.Commands.DoSomething)).Should().BeFalse();
            convention.IsQuery(typeof(Erm.Valid.Messages.Queries.GetSomethingResponse)).Should().BeFalse();
            convention.IsQueryResponse(typeof(Erm.Valid.Messages.Queries.GetSomething)).Should().BeFalse();

            //type invalid
            convention.IsEvent(typeof(Erm.Invalid.Messages.SomethingHappened)).Should().BeFalse();
            convention.IsCommand(typeof(Nedim.Invalid.Messages.Commands.DoSomething)).Should().BeFalse();
            convention.IsCommandResponse(typeof(Nedim.Invalid.Messages.Commands.DoSomethingResponse)).Should().BeFalse();
            convention.IsQuery(typeof(Erm.Invalid.Queries.GetSomethingResponse)).Should().BeFalse();
            convention.IsQueryResponse(typeof(Erm.Invalid.Queries.GetSomething)).Should().BeFalse();
        }
    }
}


namespace Erm.Valid.Messages.Events
{
    public class SomethingHappened
    {
    }
}

namespace Erm.Valid.Messages.Commands
{
    public class DoSomething
    {
    }

    public class DoSomethingResponse
    {
    }
}

namespace Erm.Valid.Messages.Queries
{
    public class GetSomething
    {
    }

    public class GetSomethingResponse
    {
    }
}

namespace Erm.Invalid.Messages
{
    public class SomethingHappened
    {
    }
}

namespace Nedim.Invalid.Messages.Commands
{
    public class DoSomething
    {
    }

    public class DoSomethingResponse
    {
    }
}

namespace Erm.Invalid.Queries
{
    public class GetSomething
    {
    }

    public class GetSomethingResponse
    {
    }
}