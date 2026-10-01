using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

public sealed class GamerPrivileges
{
    internal GamerPrivileges(CnaGamerPrivileges native)
    {
        AllowOnlineSessions = native.AllowOnlineSessions != 0;
        AllowCommunication = (GamerPrivilegeSetting)native.AllowCommunication;
        AllowProfileViewing = (GamerPrivilegeSetting)native.AllowProfileViewing;
        AllowUserCreatedContent = (GamerPrivilegeSetting)native.AllowUserCreatedContent;
        AllowTradeContent = native.AllowTradeContent != 0;
        AllowPurchaseContent = native.AllowPurchaseContent != 0;
        AllowPremiumContent = native.AllowPremiumContent != 0;
    }

    public bool AllowOnlineSessions { get; }

    public GamerPrivilegeSetting AllowCommunication { get; }

    public GamerPrivilegeSetting AllowProfileViewing { get; }

    public GamerPrivilegeSetting AllowUserCreatedContent { get; }

    public bool AllowTradeContent { get; }

    public bool AllowPurchaseContent { get; }

    public bool AllowPremiumContent { get; }
}
