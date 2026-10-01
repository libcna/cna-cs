using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.GamerServices;

[Serializable]
public class GamerPrivilegeException : Exception
{
    public GamerPrivilegeException()
    {
    }

    public GamerPrivilegeException(string message)
        : base(message)
    {
    }

    public GamerPrivilegeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#pragma warning disable SYSLIB0051 // Required by the XNA 4.0 serializable exception contract.
    protected GamerPrivilegeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#pragma warning restore SYSLIB0051
}
