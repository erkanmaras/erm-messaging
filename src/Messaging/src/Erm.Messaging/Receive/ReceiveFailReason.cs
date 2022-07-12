using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public enum ReceiveFailReason
{
    None,
    Duplicate,
    TimeToLiveExpired,
    Poison
}