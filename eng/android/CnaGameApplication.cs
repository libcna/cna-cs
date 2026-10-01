using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Android.Runtime;

namespace CNA.Android;

/// <summary>
/// The application of a CNA.NET game on Android. The game runs in SDL's activity
/// (<c>com.libcna.cna.CnaGameActivity</c>, which is SDL's <c>SDLActivity</c> unchanged): SDL owns the
/// surface and runs <c>SDL_main</c> on a thread of its own, where the window, GL context and event
/// pump belong. Android creates this object -- and so starts .NET -- before any activity, and here
/// it hands <c>libmain.so</c> (<c>eng/android/cna_android_main.c</c>) the entry point SDL's thread
/// will call. The game's unchanged <c>Main</c> then runs there and blocks in <c>Game.Run</c> as it
/// would on a desktop; when it returns, SDL finishes the activity.
/// </summary>
[SupportedOSPlatform("android24.0")]
public abstract class CnaGameApplication : global::Android.App.Application
{
    private static CnaGameApplication? _current;

    protected CnaGameApplication(nint handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {
    }

    /// <summary>Runs the game's own entry point; the app names it.</summary>
    protected abstract void RunGame();

    public override unsafe void OnCreate()
    {
        base.OnCreate();
        _current = this;
        cna_android_set_main((nint)(delegate* unmanaged[Cdecl]<int, nint, int>)&RunOnSdlThread);
    }

    [DllImport("main")]
    private static extern void cna_android_set_main(nint function);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int RunOnSdlThread(int argc, nint argv)
    {
        _ = argc;
        _ = argv;
        try
        {
            _current!.RunGame();
            return 0;
        }
        catch (System.Exception exception)
        {
            global::Android.Util.Log.Error("CNA", exception.ToString());
            return 1;
        }
    }
}
