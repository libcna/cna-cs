using System.Runtime.InteropServices;

namespace CNA.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct CnaInviteAcceptedEventInfo
{
    public uint StructSize;
    public uint StructVersion;
    public CnaHandle Gamer;
    public byte IsCurrentSession;
    public CnaReservedBytes7 Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaGamerPresence
{
    public uint StructSize;
    public uint StructVersion;
    public uint PresenceMode;
    public int PresenceValue;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaGamerPrivileges
{
    public uint StructSize;
    public uint StructVersion;
    public uint AllowCommunication;
    public uint AllowProfileViewing;
    public uint AllowUserCreatedContent;
    public byte AllowOnlineSessions;
    public byte AllowPremiumContent;
    public byte AllowPurchaseContent;
    public byte AllowTradeContent;
    public CnaReservedBytes4 Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaGamerProfileInfo
{
    public uint StructSize;
    public uint StructVersion;
    public int GamerScore;
    public uint GamerZone;
    public int TitlesPlayed;
    public int TotalAchievements;
    public float Reputation;
    public byte IsDisposed;
    public CnaReservedBytes3 Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaFriendGamerInfo
{
    public uint StructSize;
    public uint StructVersion;
    public byte FriendRequestReceivedFrom;
    public byte FriendRequestSentTo;
    public byte HasVoice;
    public byte InviteAccepted;
    public byte InviteReceivedFrom;
    public byte InviteRejected;
    public byte InviteSentTo;
    public byte IsAway;
    public byte IsBusy;
    public byte IsJoinable;
    public byte IsOnline;
    public byte IsPlaying;
    public CnaReservedBytes4 Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaSignedInGamerEventInfo
{
    public uint StructSize;
    public uint StructVersion;
    public uint Reserved;
    public CnaHandle Gamer;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaAchievementInfo
{
    public uint StructSize;
    public uint StructVersion;
    public int GamerScore;
    public byte DisplayBeforeEarned;
    public byte EarnedOnline;
    public byte IsEarned;
    public byte Reserved;
    public long EarnedDateTimeTicks;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaGameDefaults
{
    public uint StructSize;
    public uint StructVersion;
    public uint GameDifficulty;
    public uint ControllerSensitivity;
    public uint RacingCameraAngle;
    public byte HasPrimaryColor;
    public byte HasSecondaryColor;
    public byte AutoAim;
    public byte AutoCenter;
    public byte MoveWithRightThumbStick;
    public byte InvertYAxis;
    public byte ManualTransmission;
    public byte AccelerateWithButtons;
    public byte BrakeWithButtons;
    public CnaReservedBytes3 Reserved;
    public CnaColor PrimaryColor;
    public CnaColor SecondaryColor;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaLeaderboardIdentity
{
    public uint StructSize;
    public uint StructVersion;
    public int GameMode;
    public CnaChars64 Key;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaLeaderboardReaderInfo
{
    public uint StructSize;
    public uint StructVersion;
    public int PageStart;
    public int TotalLeaderboardSize;
    public int EntryCount;
    public byte IsDisposed;
    public byte CanPageDown;
    public byte CanPageUp;
    public byte Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaLeaderboardEntryInfo
{
    public uint StructSize;
    public uint StructVersion;
    public int Ranking;
    public byte HasGamer;
    public CnaReservedBytes3 Reserved;
    public long Rating;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaAvatarExpression
{
    public uint StructSize;
    public uint StructVersion;
    public uint Mouth;
    public uint LeftEye;
    public uint RightEye;
    public uint LeftEyebrow;
    public uint RightEyebrow;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaAvatarDescriptionInfo
{
    public uint StructSize;
    public uint StructVersion;
    public uint BodyType;
    public float Height;
    public ulong DescriptionByteCount;
    public byte IsValid;
    public CnaReservedBytes7 Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaAvatarAnimationInfo
{
    public uint StructSize;
    public uint StructVersion;
    public int BoneTransformCount;
    public byte IsDisposed;
    public CnaReservedBytes3 Reserved;
    public long CurrentPositionTicks;
    public long LengthTicks;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaAvatarRendererInfo
{
    public uint StructSize;
    public uint StructVersion;
    public uint State;
    public byte IsDisposed;
    public CnaReservedBytes3 Reserved;
}

/// <summary>Four reserved padding bytes, spelled like <see cref="CnaReservedBytes3"/>.</summary>
[System.Runtime.CompilerServices.InlineArray(4)]
internal struct CnaReservedBytes4
{
    private byte _element0;
}

/// <summary>Six reserved padding bytes, spelled like <see cref="CnaReservedBytes3"/>.</summary>
[System.Runtime.CompilerServices.InlineArray(6)]
internal struct CnaReservedBytes6
{
    private byte _element0;
}

/// <summary><c>char[64]</c>: <c>CNA_LeaderboardIdentity.key</c>, a NUL-terminated UTF-8 key.</summary>
[System.Runtime.CompilerServices.InlineArray(64)]
internal struct CnaChars64
{
    private byte _element0;
}
