using System;

namespace Erm.Messaging.KafkaTransport.IntegrationTests;

public static class GuidExtensions
{
    public static string ToShortString(this Guid guid)
    {
        var base64Guid = Convert.ToBase64String(guid.ToByteArray());

        // Replace URL unfriendly characters with better ones
        base64Guid = base64Guid.Replace("+", "").Replace("/", "");

        // Remove the trailing ==
        return base64Guid[..^2];
    }
}