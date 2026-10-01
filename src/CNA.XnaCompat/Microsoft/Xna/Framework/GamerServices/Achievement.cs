using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

public sealed unsafe class Achievement
{
    private readonly NativeResourceHandle _handle;

    internal Achievement(CnaHandle handle)
    {
        _handle = new NativeResourceHandle(
            handle.AsNint, value => Native.cna_achievement_destroy(new CnaHandle(value)).IsSuccess());
        Key = GamerServicesInterop.ReadString(Native.cna_achievement_get_key_size, CopyKey, Handle, nameof(Key));
    }

    internal CnaHandle Handle => new(_handle.DangerousGetHandle());

    public string Key { get; }

    public string Name => GamerServicesInterop.ReadString(
        Native.cna_achievement_get_name_size, CopyName, Handle, nameof(Name));

    public string Description => GamerServicesInterop.ReadString(
        Native.cna_achievement_get_description_size, CopyDescription, Handle, nameof(Description));

    public string HowToEarn => GamerServicesInterop.ReadString(
        Native.cna_achievement_get_how_to_earn_size, CopyHowToEarn, Handle, nameof(HowToEarn));

    public int GamerScore => Info().GamerScore;

    public bool IsEarned => Info().IsEarned != 0;

    public bool EarnedOnline => Info().EarnedOnline != 0;

    public bool DisplayBeforeEarned => Info().DisplayBeforeEarned != 0;

    public DateTime EarnedDateTime => new(Info().EarnedDateTimeTicks);

    public Stream GetPicture()
    {
        byte[] bytes = GamerServicesInterop.ReadBytes(
            Native.cna_achievement_get_picture_size, CopyPicture, Handle, nameof(GetPicture));
        return bytes.Length == 0 ? null! : new MemoryStream(bytes, writable: false);
    }

    private CnaAchievementInfo Info()
    {
        CnaAchievementInfo info = GamerServicesInterop.Versioned<CnaAchievementInfo>();
        GamerServicesInterop.Check(Native.cna_achievement_get_info(Handle, ref info), nameof(Achievement));
        return info;
    }

    private static unsafe CnaResult CopyKey(CnaHandle h, byte* d, ulong c, out ulong b) => Native.cna_achievement_copy_key(h, d, c, out b);

    private static unsafe CnaResult CopyName(CnaHandle h, byte* d, ulong c, out ulong b) => Native.cna_achievement_copy_name(h, d, c, out b);

    private static unsafe CnaResult CopyDescription(CnaHandle h, byte* d, ulong c, out ulong b) => Native.cna_achievement_copy_description(h, d, c, out b);

    private static unsafe CnaResult CopyHowToEarn(CnaHandle h, byte* d, ulong c, out ulong b) => Native.cna_achievement_copy_how_to_earn(h, d, c, out b);

    private static unsafe CnaResult CopyPicture(CnaHandle h, byte* d, ulong c, out ulong b) => Native.cna_achievement_copy_picture(h, d, c, out b);
}
