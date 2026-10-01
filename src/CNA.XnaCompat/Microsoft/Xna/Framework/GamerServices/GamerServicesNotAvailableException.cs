using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.GamerServices;

[Serializable]
public class GamerServicesNotAvailableException : Exception
{
    public GamerServicesNotAvailableException()
    {
    }

    public GamerServicesNotAvailableException(string message)
        : base(message)
    {
    }

    public GamerServicesNotAvailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#pragma warning disable SYSLIB0051 // Required by the XNA 4.0 serializable exception contract.
    protected GamerServicesNotAvailableException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#pragma warning restore SYSLIB0051
}
