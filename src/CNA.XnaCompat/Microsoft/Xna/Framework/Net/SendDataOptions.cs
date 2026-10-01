namespace Microsoft.Xna.Framework.Net;

[Flags]
public enum SendDataOptions
{
    None = 0,
    Reliable = 1,
    InOrder = 2,
    ReliableInOrder = 3,
    Chat = 4,
}
