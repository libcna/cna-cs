using System;
using System.Collections.Generic;
using System.IO;
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
    private const string ExtractedStampFile = ".cna-title-assets";

    private static CnaGameApplication? _current;

    protected CnaGameApplication(nint handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {
    }

    /// <summary>Runs the game's own entry point; the app names it.</summary>
    protected abstract void RunGame();

    /// <summary>
    /// The APK asset directories that hold title files, extracted beside the game before it runs:
    /// XNA's <c>Content</c> by default.
    /// </summary>
    protected virtual IEnumerable<string> TitleAssetDirectories => ["Content"];

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
            _current!.ExtractTitleAssets();
            // Windows starts a game in its own folder, and XNA reads some paths against the working
            // directory (AudioEngine's settings file, Path.GetFullPath); here it would be "/".
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            _current.RunGame();
            return 0;
        }
        catch (System.Exception exception)
        {
            global::Android.Util.Log.Error("CNA", exception.ToString());
            return 1;
        }
    }

    /// <summary>
    /// Makes the title's files files. An APK keeps them as assets, which CNA's native loaders read
    /// but <see cref="System.IO"/> cannot: XNA's managed content readers, <c>TitleContainer</c> and a
    /// game's own <c>File.OpenRead("Content/...")</c> all look in the title directory, which on
    /// Android is the app's files directory. They are copied there once per installed APK -- the
    /// APK's path changes with every install and update -- on SDL's thread rather than the UI
    /// thread, so a large title cannot stall the activity's start.
    /// </summary>
    private void ExtractTitleAssets()
    {
        string root = AppContext.BaseDirectory;
        string stampPath = Path.Combine(root, ExtractedStampFile);
        string stamp = ApplicationInfo?.SourceDir ?? string.Empty;
        if (File.Exists(stampPath) && File.ReadAllText(stampPath) == stamp)
        {
            return;
        }

        global::Android.Content.Res.AssetManager assets = Assets!;
        long files = 0, bytes = 0;
        foreach (string directory in TitleAssetDirectories)
        {
            ExtractAssetTree(assets, directory, root, ref files, ref bytes);
        }
        File.WriteAllText(stampPath, stamp);
        global::Android.Util.Log.Info("CNA", $"Extracted {files} title files ({bytes} bytes) to {root}.");
    }

    private static void ExtractAssetTree(
        global::Android.Content.Res.AssetManager assets, string path, string root, ref long files, ref long bytes)
    {
        string[] children = assets.List(path) ?? [];
        if (children.Length > 0)
        {
            foreach (string child in children)
            {
                ExtractAssetTree(assets, path + "/" + child, root, ref files, ref bytes);
            }
            return;
        }

        // A path with no children is a file or an empty directory; only a file opens.
        Stream source;
        try
        {
            source = assets.Open(path);
        }
        catch (Java.IO.FileNotFoundException)
        {
            return;
        }

        string destination = Path.Combine(root, path);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        using (source)
        using (FileStream target = File.Create(destination))
        {
            source.CopyTo(target);
            files++;
            bytes += target.Length;
        }
    }
}
