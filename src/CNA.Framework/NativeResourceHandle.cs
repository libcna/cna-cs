using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace CNA;

/// <summary>
/// The owner of one CNA native resource, parameterized by the release callback for the specific
/// resource type. Every native-backed CNA type (<c>Texture2D</c>, <c>SpriteBatch</c>, ...) owns
/// one of these rather than a bare handle value, so normal disposal, forgotten disposal, and GC
/// finalization are all handled uniformly. CNA handles are creation-thread-affine, so
/// finalizer releases are queued and drained by the owning game thread rather than attempted from
/// the finalizer thread. See plan.md invariant #4.
///
/// Not a <see cref="SafeHandle"/>: a <c>CNA_Handle</c> is a 64-bit value on every platform, and
/// <see cref="SafeHandle"/> stores a pointer-width one, so on WebAssembly (and any 32-bit target)
/// it cannot hold a CNA handle at all. This keeps <see cref="SafeHandle"/>'s contract -- release at
/// most once, from <see cref="Dispose"/> or a critical finalizer, never for a borrowed or detached
/// handle -- over the real 64-bit value.
/// </summary>
internal sealed class NativeResourceHandle : CriticalFinalizerObject, IDisposable
{
    private const int Open = 0;
    private const int Closed = 1;

    private readonly Func<ulong, bool> _release;
    private readonly int _ownerThreadId;
    private readonly bool _ownsHandle;
    private ulong _handle;
    private int _state;

    private static readonly object PendingLock = new();
    private static readonly Dictionary<int, Queue<PendingRelease>> PendingByOwnerThread = [];
    private static long _queuedOwnerThreadReleases;
    private static long _releaseAttempts;
    private static long _successfulReleases;
    private static long _failedReleaseAttempts;
    private static long _scheduledRetries;

    public NativeResourceHandle(ulong handleValue, Func<ulong, bool> release)
        : this(handleValue, release, ownsHandle: true)
    {
    }

    /// <summary>
    /// <paramref name="ownsHandle"/> <see langword="false"/> wraps a handle this object must
    /// <em>not</em> release -- the C API hands several out as explicitly borrowed, with a lifetime
    /// the real owner controls (<c>cna_video_player_get_texture</c>'s frame texture is the case
    /// this was added for: "valid only until the next call on this player").
    ///
    /// Without it, a borrowed handle wrapped here would be destroyed by the finalizer whether or
    /// not anyone called <c>Dispose</c> -- a use-after-free the owner could not prevent, and one a
    /// doc comment telling callers "do not dispose this" cannot stop either.
    /// </summary>
    public NativeResourceHandle(ulong handleValue, Func<ulong, bool> release, bool ownsHandle)
    {
        _release = release;
        _ownerThreadId = Environment.CurrentManagedThreadId;
        _ownsHandle = ownsHandle;
        _handle = handleValue;
    }

    ~NativeResourceHandle()
    {
        CloseAndRelease();
    }

    public bool IsInvalid => Volatile.Read(ref _handle) == 0;

    public bool IsClosed => Volatile.Read(ref _state) == Closed;

    /// <summary>The native handle value. Valid only while this object is open; keep the owner
    /// reachable across the native call that uses it.</summary>
    public ulong DangerousGetHandle() => Volatile.Read(ref _handle);

    public void Dispose()
    {
        CloseAndRelease();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gives up ownership: returns the handle value and marks this object closed, so neither
    /// <c>Dispose</c> nor the finalizer will ever release it.
    ///
    /// For the narrow case where a managed wrapper exists only to perform one operation and the
    /// resulting handle then belongs to something else -- <c>ContentManager</c> builds a
    /// <c>Texture2D</c> to upload a SpriteFont atlas, then hands the handle to the
    /// <c>SpriteFont</c>'s own texture. Without this, both wrappers would own the same handle and
    /// the first one's finalizer would destroy a texture the font is still drawing from.
    /// </summary>
    public ulong Detach()
    {
        ulong value = Volatile.Read(ref _handle);
        Volatile.Write(ref _state, Closed);
        GC.SuppressFinalize(this);
        return value;
    }

    private void CloseAndRelease()
    {
        if (Interlocked.Exchange(ref _state, Closed) == Closed || !_ownsHandle || IsInvalid)
        {
            return;
        }

        ReleaseHandle();
    }

    private void ReleaseHandle()
    {
        var pending = new PendingRelease(_handle, _release);
        if (Environment.CurrentManagedThreadId == _ownerThreadId)
        {
            if (!TryRelease(pending))
            {
                Interlocked.Increment(ref _scheduledRetries);
                Enqueue(_ownerThreadId, pending);
            }

            return;
        }

        // CNA's registry rejects every Get/Release from a thread other than the handle's creation
        // thread. A finalizer necessarily runs on the finalizer thread, so calling native here used
        // to return CNA_RESULT_THREAD and permanently lose the only copy of the handle. Queue the
        // value and release delegate for the owning game thread.
        Enqueue(_ownerThreadId, pending);
    }

    /// <summary>
    /// Releases handles whose finalizers ran away from their creation thread. Failed
    /// releases are retried after successful ones in the same batch: this handles parent/child
    /// order without guessing finalizer order (for example an effect view before its effect, or a
    /// texture retained by a batch). Anything still failing is retained for the next owner-thread
    /// safe point rather than silently leaked.
    /// </summary>
    internal static void DrainPendingReleasesForCurrentThread()
    {
        int ownerThreadId = Environment.CurrentManagedThreadId;
        List<PendingRelease> pending;
        lock (PendingLock)
        {
            if (!PendingByOwnerThread.Remove(ownerThreadId, out Queue<PendingRelease>? queue))
            {
                return;
            }

            pending = [.. queue];
        }

        while (pending.Count > 0)
        {
            var failed = new List<PendingRelease>();
            bool madeProgress = false;
            foreach (PendingRelease release in pending)
            {
                if (TryRelease(release))
                {
                    madeProgress = true;
                }
                else
                {
                    failed.Add(release);
                }
            }

            if (failed.Count == 0)
            {
                return;
            }

            Interlocked.Add(ref _scheduledRetries, failed.Count);

            if (!madeProgress)
            {
                lock (PendingLock)
                {
                    if (!PendingByOwnerThread.TryGetValue(ownerThreadId, out Queue<PendingRelease>? queue))
                    {
                        queue = new Queue<PendingRelease>();
                        PendingByOwnerThread.Add(ownerThreadId, queue);
                    }

                    foreach (PendingRelease release in failed)
                    {
                        queue.Enqueue(release);
                    }
                }

                return;
            }

            pending = failed;
        }
    }

    private static bool TryRelease(PendingRelease pending)
    {
        Interlocked.Increment(ref _releaseAttempts);
        try
        {
            if (pending.Release(pending.Handle))
            {
                Interlocked.Increment(ref _successfulReleases);
                return true;
            }
        }
        catch
        {
            // A release cannot throw, particularly from the finalizer path. Retain
            // the work so a later owner-thread drain can retry it.
        }

        Interlocked.Increment(ref _failedReleaseAttempts);
        return false;
    }

    private static void Enqueue(int ownerThreadId, PendingRelease pending)
    {
        lock (PendingLock)
        {
            if (!PendingByOwnerThread.TryGetValue(ownerThreadId, out Queue<PendingRelease>? queue))
            {
                queue = new Queue<PendingRelease>();
                PendingByOwnerThread.Add(ownerThreadId, queue);
            }

            queue.Enqueue(pending);
            Interlocked.Increment(ref _queuedOwnerThreadReleases);
        }
    }

    internal static NativeReleaseMetrics GetMetrics()
    {
        long pending;
        lock (PendingLock)
        {
            pending = PendingByOwnerThread.Values.Sum(static queue => (long)queue.Count);
        }

        return new NativeReleaseMetrics(
            Interlocked.Read(ref _queuedOwnerThreadReleases),
            Interlocked.Read(ref _releaseAttempts),
            Interlocked.Read(ref _successfulReleases),
            Interlocked.Read(ref _failedReleaseAttempts),
            Interlocked.Read(ref _scheduledRetries),
            pending);
    }

    private readonly record struct PendingRelease(ulong Handle, Func<ulong, bool> Release);
}

internal readonly record struct NativeReleaseMetrics(
    long QueuedOwnerThreadReleases,
    long ReleaseAttempts,
    long SuccessfulReleases,
    long FailedReleaseAttempts,
    long ScheduledRetries,
    long PendingOwnerThreadReleases)
{
    public static NativeReleaseMetrics operator -(NativeReleaseMetrics end, NativeReleaseMetrics start) =>
        new(
            end.QueuedOwnerThreadReleases - start.QueuedOwnerThreadReleases,
            end.ReleaseAttempts - start.ReleaseAttempts,
            end.SuccessfulReleases - start.SuccessfulReleases,
            end.FailedReleaseAttempts - start.FailedReleaseAttempts,
            end.ScheduledRetries - start.ScheduledRetries,
            end.PendingOwnerThreadReleases);
}
