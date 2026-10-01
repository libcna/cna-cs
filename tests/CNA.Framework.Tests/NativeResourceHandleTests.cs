using System.Runtime.CompilerServices;
using Xunit;

namespace CNA.Framework.Tests;

public sealed class NativeResourceHandleTests
{
    [Fact]
    public void CriticalFinalizer_DefersReleaseToCreationThread()
    {
        var counter = new Counter();
        WeakReference reference = AllocateAndAbandon(counter);

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

        Assert.False(reference.IsAlive);
        Assert.Equal(0, counter.Value);

        NativeResourceHandle.DrainPendingReleasesForCurrentThread();
        Assert.Equal(1, counter.Value);
    }

    /// <summary>
    /// A <c>CNA_Handle</c> is 64-bit on every platform. The owner used to be a
    /// <see cref="System.Runtime.InteropServices.SafeHandle"/>, which stores a pointer-width value:
    /// on WebAssembly every handle above 2^31 overflowed on the way in, and the browser build died
    /// creating its first content manager. The first assertion holds on any runtime; the type
    /// assertions are what keep a 32-bit runtime working.
    /// </summary>
    [Fact]
    public void Handle_HoldsTheFull64BitValue()
    {
        const ulong value = 0xFEDC_BA98_0000_0001UL;
        ulong released = 0;
        var handle = new NativeResourceHandle(value, h =>
        {
            released = h;
            return true;
        });

        Assert.Equal(value, handle.DangerousGetHandle());
        handle.Dispose();
        Assert.Equal(value, released);

        Assert.False(typeof(System.Runtime.InteropServices.SafeHandle).IsAssignableFrom(typeof(NativeResourceHandle)));
        Assert.Equal(typeof(ulong), typeof(NativeResourceHandle).GetMethod(nameof(NativeResourceHandle.DangerousGetHandle))!.ReturnType);
    }

    /// <summary>What <see cref="System.Runtime.InteropServices.SafeHandle"/> guaranteed and the
    /// replacement keeps: a release at most once, none for a borrowed or detached handle, and a
    /// closed state the wrappers' <c>IsDisposed</c> reads.</summary>
    [Fact]
    public void Handle_ReleasesAtMostOnceAndNeverABorrowedOrDetachedOne()
    {
        int owned = 0, borrowed = 0, detached = 0;
        var ownedHandle = new NativeResourceHandle(7UL, _ => { owned++; return true; });
        var borrowedHandle = new NativeResourceHandle(8UL, _ => { borrowed++; return true; }, ownsHandle: false);
        var detachedHandle = new NativeResourceHandle(9UL, _ => { detached++; return true; });

        Assert.False(ownedHandle.IsClosed);
        ownedHandle.Dispose();
        ownedHandle.Dispose();
        borrowedHandle.Dispose();
        Assert.Equal(9UL, detachedHandle.Detach());
        detachedHandle.Dispose();

        Assert.True(ownedHandle.IsClosed);
        Assert.True(detachedHandle.IsClosed);
        Assert.Equal((1, 0, 0), (owned, borrowed, detached));
        Assert.True(new NativeResourceHandle(0UL, _ => true).IsInvalid);
    }

    [Fact]
    public void CrossThreadDispose_DefersReleaseToCreationThread()
    {
        int releases = 0;
        using var handle = new NativeResourceHandle(1UL, _ =>
        {
            releases++;
            return true;
        });

        var thread = new Thread(handle.Dispose);
        thread.Start();
        thread.Join();

        Assert.Equal(0, releases);
        NativeResourceHandle.DrainPendingReleasesForCurrentThread();
        Assert.Equal(1, releases);
    }

    [Fact]
    public void Drain_RetriesFailedParentAfterChildRelease()
    {
        bool childReleased = false;
        int parentAttempts = 0;
        int childAttempts = 0;
        using var parent = new NativeResourceHandle(1UL, _ =>
        {
            parentAttempts++;
            return childReleased;
        });
        using var child = new NativeResourceHandle(2UL, _ =>
        {
            childAttempts++;
            childReleased = true;
            return true;
        });

        var thread = new Thread(() =>
        {
            parent.Dispose();
            child.Dispose();
        });
        thread.Start();
        thread.Join();

        NativeResourceHandle.DrainPendingReleasesForCurrentThread();

        Assert.Equal(2, parentAttempts);
        Assert.Equal(1, childAttempts);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AllocateAndAbandon(Counter counter)
    {
        var handle = new NativeResourceHandle(1UL, _ =>
        {
            counter.Value++;
            return true;
        });
        return new WeakReference(handle);
    }

    private sealed class Counter
    {
        public int Value { get; set; }
    }
}
