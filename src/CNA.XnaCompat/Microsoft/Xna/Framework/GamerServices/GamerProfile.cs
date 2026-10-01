using System.Globalization;
using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

public sealed unsafe class GamerProfile : IDisposable
{
    private readonly NativeResourceHandle _handle;

    internal GamerProfile(CnaHandle handle)
    {
        _handle = new NativeResourceHandle(
            handle.AsNint, value => Native.cna_gamer_profile_destroy(new CnaHandle(value)).IsSuccess());
    }

    private CnaHandle Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle.IsClosed, this);
            return new CnaHandle(_handle.DangerousGetHandle());
        }
    }

    public bool IsDisposed => _handle.IsClosed;

    public string Motto => GamerServicesInterop.ReadString(
        Native.cna_gamer_profile_get_motto_size, CopyMotto, Handle, nameof(Motto));

    public float Reputation => Info().Reputation;

    public GamerZone GamerZone => (GamerZone)Info().GamerZone;

    /// <summary>The profile's region, or null when native names none this runtime recognizes.</summary>
    public RegionInfo Region
    {
        get
        {
            string name = GamerServicesInterop.ReadString(
                Native.cna_gamer_profile_get_region_name_size, CopyRegionName, Handle, nameof(Region));
            if (string.IsNullOrEmpty(name))
            {
                return null!;
            }

            try
            {
                return new RegionInfo(name);
            }
            catch (ArgumentException)
            {
                return null!;
            }
        }
    }

    public int GamerScore => Info().GamerScore;

    public int TitlesPlayed => Info().TitlesPlayed;

    public int TotalAchievements => Info().TotalAchievements;

    public unsafe Stream GetGamerPicture()
    {
        GamerServicesInterop.Check(
            Native.cna_gamer_profile_get_picture_size(Handle, out byte hasPicture, out ulong size), nameof(GetGamerPicture));
        if (hasPicture == 0)
        {
            return null!;
        }

        byte[] bytes = new byte[size];
        fixed (byte* pointer = bytes)
        {
            GamerServicesInterop.Check(
                Native.cna_gamer_profile_copy_picture(Handle, pointer, size, out ulong written), nameof(GetGamerPicture));
            return new MemoryStream(bytes, 0, (int)written, writable: false);
        }
    }

    public void Dispose()
    {
        _handle.Dispose();
        GC.SuppressFinalize(this);
    }

    private CnaGamerProfileInfo Info()
    {
        CnaGamerProfileInfo info = SignedInGamer.Versioned<CnaGamerProfileInfo>();
        GamerServicesInterop.Check(Native.cna_gamer_profile_get_info(Handle, ref info), nameof(GamerProfile));
        return info;
    }

    private static unsafe CnaResult CopyMotto(CnaHandle handle, byte* destination, ulong capacity, out ulong bytes) =>
        Native.cna_gamer_profile_copy_motto(handle, destination, capacity, out bytes);

    private static unsafe CnaResult CopyRegionName(CnaHandle handle, byte* destination, ulong capacity, out ulong bytes) =>
        Native.cna_gamer_profile_copy_region_name(handle, destination, capacity, out bytes);
}
