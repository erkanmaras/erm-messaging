namespace Erm.Messaging.KafkaTransport;

public class KafkaConsumerConfiguration
{
    internal KafkaConsumerConfiguration(string name, string[] topicNames)
    {
        Name = name;
        TopicNames = topicNames;
    }

    internal string Name { get; }
    internal string[] TopicNames { get; }

    public string? GroupId { get; set; }

    //TODO : Optimum default worker count kaç olmalı ?
    public int WorkersCount { get; set; } = 10;

    //TODO : Optimum default worker buffer kaç olmalı ?
    public int WorkerBufferCount { get; set; } = 25;
}