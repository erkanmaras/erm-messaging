#nullable disable
namespace Erm.Messaging;

public static class MessageContentTypes
{
    /// <summary>
    /// "application/json";
    /// </summary>
    public const string Default = Json;

    /// <summary>
    /// "application/json";
    /// </summary>
    public const string Json = "application/json";

    /// <summary>
    /// "application/protobuf"
    /// </summary>
    public const string Protobuf = "application/protobuf";
}