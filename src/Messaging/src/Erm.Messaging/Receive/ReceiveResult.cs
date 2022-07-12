using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class ReceiveResult
{
    public bool Succeeded { get; private set; } = true;
    public ReceiveFailReason FailReason { get; private set; } = ReceiveFailReason.None;

    public void SetSucceeded()
    {
        Succeeded = true;
        FailReason = ReceiveFailReason.None;
    }

    public void SetFaulted(ReceiveFailReason reason)
    {
        Succeeded = false;
        FailReason = reason;
    }
}