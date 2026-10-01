using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.GamerServices;

[Serializable]
public class GuideAlreadyVisibleException : Exception
{
    public GuideAlreadyVisibleException()
    {
    }

    public GuideAlreadyVisibleException(string message)
        : base(message)
    {
    }

    public GuideAlreadyVisibleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#pragma warning disable SYSLIB0051 // Required by the XNA 4.0 serializable exception contract.
    protected GuideAlreadyVisibleException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#pragma warning restore SYSLIB0051
}
