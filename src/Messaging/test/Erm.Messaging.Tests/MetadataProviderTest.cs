using System;
using FluentAssertions;
using Xunit;

// ReSharper disable All
namespace Erm.Messaging.Tests;

public class MetadataProviderTest
{
    [Fact]
    public void AssemblyScan_ShouldResolve_MessageTypes_With_CustomConvention()
    {
        var metadataProvider = new MetadataProvider(
            scanMessagesIn: new[] { GetType().Assembly },
            messageTypeConvention: new CustomMessageTypeConvention());
        metadataProvider.GetMessageObjectTypes().Should().HaveCount(4);
        metadataProvider.GetMessageObjectTypes().Should().Contain(new[] { typeof(Event1), typeof(Event2), typeof(Command1), typeof(Command2) });
        metadataProvider.GetMessageType(typeof(Event1).FullName).Should().Be(MessageType.Event);
        metadataProvider.GetMessageType(typeof(Command1).FullName).Should().Be(MessageType.Command);
    }

    [Fact]
    public void AddEventType_ShouldRegister_Event()
    {
        var metadataProvider = new MetadataProvider();
        metadataProvider.AddEventType<Event1>();
        metadataProvider.GetMessageObjectTypes().Should().Contain(new[] { typeof(Event1) });
        metadataProvider.GetMessageType(typeof(Event1).FullName).Should().Be(MessageType.Event);
    }

    [Fact]
    public void AddCommandType_ShouldRegister_Command()
    {
        var metadataProvider = new MetadataProvider();
        metadataProvider.AddCommandType<Command1>();
        metadataProvider.GetMessageObjectTypes().Should().Contain(new[] { typeof(Command1) });
        metadataProvider.GetMessageType(typeof(Command1).FullName).Should().Be(MessageType.Command);
    }


    [Fact]
    public void GetDestinationAddress_ShouldReturn_ConfiguredAddress()
    {
        var metadataProvider = new MetadataProvider(messageSendMetadata: new CustomSendMetadata());
        metadataProvider.AddEventType<Event1>();
        var envelope = new Envelope<Event1>(new Event1());
        metadataProvider.GetDestinationAddress(envelope.MessageName).Should().Be(CustomSendMetadata.Event1Destination);
    }

    [Fact]
    public void GetDestinationAddress_ShouldReturn_DefaultEventAddress_IfNoAddressConfigured()
    {
        var metadataProvider = new MetadataProvider(messageSendMetadata: new CustomSendMetadata());
        metadataProvider.AddEventType<Event2>();
        var envelope = new Envelope<Event2>(new Event2());
        metadataProvider.GetDestinationAddress(envelope.MessageName).Should().Be(CustomSendMetadata.DefaultEventDestination);
    }

    [Fact]
    public void GetDestinationAddress_ShouldThrow_IfNoAddressConfigured()
    {
        var metadataProvider = new MetadataProvider(messageSendMetadata: new CustomSendMetadata());
        metadataProvider.AddEventType<Event1>();
        var envelope = new Envelope<Command1>(new Command1());
        metadataProvider.Invoking(m => m.GetDestinationAddress(envelope.MessageName)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetContentType_ShouldReturn_ConfiguredContentType()
    {
        var metadataProvider = new MetadataProvider(messageSendMetadata: new CustomSendMetadata());
        metadataProvider.AddEventType<Event1>();
        var envelope = new Envelope<Event1>(new Event1());
        metadataProvider.GetContentType(envelope.MessageName).Should().Be(MessageContentTypes.Protobuf);
    }

    [Fact]
    public void GetContentType_ShouldReturn_Json_IfNoContentTypeConfigured()
    {
        var metadataProvider = new MetadataProvider(messageSendMetadata: new CustomSendMetadata());
        metadataProvider.AddEventType<Event2>();
        var envelope = new Envelope<Event2>(new Event2());
        metadataProvider.GetContentType(envelope.MessageName).Should().Be(MessageContentTypes.Json);
    }

    [Fact]
    public void GetGetReplyToAddress_ShouldReturn_ConfiguredAddress()
    {
        var metadataProvider = new MetadataProvider(messageReceiveMetadata: new CustomReceiveMetadata());
        metadataProvider.AddCommandType<Command1>();
        var envelope = new Envelope<Command1>(new Command1());
        metadataProvider.GetReplyToAddress(envelope.MessageName).Should().Be(CustomReceiveMetadata.CommandResponseDestination);
    }

    [Fact]
    public void GetGetReplyToAddress_ShouldReturnEmptyString_IfMessageTypeNotCommandOrQuery()
    {
        var metadataProvider = new MetadataProvider(messageReceiveMetadata: new CustomReceiveMetadata());
        metadataProvider.AddCommandType<Event1>();
        var envelope = new Envelope<Event1>(new Event1());
        metadataProvider.GetReplyToAddress(envelope.MessageName).Should().Be(CustomReceiveMetadata.CommandResponseDestination);
    }

    public class Event1
    {
        public string Version { get; }
        public Guid Id { get; }
    }

    public class Event2
    {
        public string Version { get; }
        public Guid Id { get; }
    }

    public class Command1
    {
        public string Version { get; }
        public Guid Id { get; }
    }

    public class Command2
    {
        public string Version { get; }
        public Guid Id { get; }
    }

    private class CustomMessageTypeConvention : IMessageTypeConvention
    {
        public bool IsEvent(Type type)
        {
            return type == typeof(Event1) || type == typeof(Event2);
        }

        public bool IsCommand(Type type)
        {
            return type == typeof(Command1) || type == typeof(Command2);
        }

        public bool IsCommandResponse(Type type)
        {
            return false;
        }

        public bool IsQuery(Type type)
        {
            return false;
        }

        public bool IsQueryResponse(Type type)
        {
            return false;
        }
    }

    private class CustomSendMetadata : SendMetadata
    {
        public const string Event1Destination = "Event1Destination";
        public const string DefaultEventDestination = "DefaultEventDestination";

        public override string GetEventsDefaultDestination(string messageDomain)
        {
            return DefaultEventDestination;
        }

        public CustomSendMetadata()
        {
            AddSendMetadata<Event1>(Event1Destination, MessageContentTypes.Protobuf);
        }
    }

    private class CustomReceiveMetadata : ReceiveMetadata
    {
        public const string CommandResponseDestination = "CommandResponseDestination";
        public const string QueryResponseDestination = "QueryResponseDestination";

        public override string GetCommandResponseDestination(string messageDomain)
        {
            return CommandResponseDestination;
        }

        public override string GetQueryResponseDestination(string messageDomain)
        {
            return QueryResponseDestination;
        }
    }
}