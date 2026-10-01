using System.Runtime.Serialization;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net;

[Serializable]
public class NetworkSessionJoinException : NetworkException
{
    public NetworkSessionJoinException()
    {
    }

    public NetworkSessionJoinException(string message)
        : base(message)
    {
    }

    public NetworkSessionJoinException(string message, NetworkSessionJoinError joinError)
        : base(message)
    {
        JoinError = joinError;
    }

    public NetworkSessionJoinException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#pragma warning disable SYSLIB0051, CS0672 // Required by the XNA 4.0 serializable exception contract.
    protected NetworkSessionJoinException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        JoinError = (NetworkSessionJoinError)info.GetInt32(nameof(JoinError));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(JoinError), (int)JoinError);
    }
#pragma warning restore SYSLIB0051, CS0672

    public NetworkSessionJoinError JoinError { get; set; }
}
