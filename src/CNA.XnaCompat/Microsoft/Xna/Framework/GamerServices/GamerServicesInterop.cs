using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>
/// What GamerServices, Avatar and Net share on their way to the C ABI. These subsystems exist only to
/// satisfy XNA, so they call <see cref="Native"/> directly rather than through a CNA.* layer of their
/// own (docs/architecture.md).
/// </summary>
internal static class GamerServicesInterop
{
    /// <summary>
    /// Turns a native refusal into the exception XNA throws for the same call. Native reports the
    /// category; the XNA type is the category's, and the native message is kept. Anything that is
    /// not one of the four categories XNA has an answer for stays a <see cref="CnaException"/>.
    /// </summary>
    internal static void Check(CnaResult result, string operation)
    {
        if (result.IsSuccess())
        {
            return;
        }

        string detail = CnaError.GetLastErrorMessage();
        string message = string.IsNullOrEmpty(detail) ? $"{operation} failed with {result}." : detail;
        throw result switch
        {
            CnaResult.InvalidArgument => new ArgumentException(message),
            CnaResult.InvalidState => new InvalidOperationException(message),
            CnaResult.NotSupported => new NotSupportedException(message),
            CnaResult.InvalidHandle => new ObjectDisposedException(operation, message),
            _ => CnaExceptionFor(result, operation),
        };
    }

    private static Exception CnaExceptionFor(CnaResult result, string operation)
    {
        try
        {
            CnaException.ThrowIfFailed(result, operation);
        }
        catch (CnaException exception)
        {
            return exception;
        }

        return new InvalidOperationException(operation);
    }

    internal delegate CnaResult GlobalSize(out ulong bytes);

    internal unsafe delegate CnaResult GlobalCopy(byte* destination, ulong capacity, out ulong bytes);

    /// <summary>The size-then-copy pair for a string that belongs to no handle.</summary>
    internal static unsafe string ReadString(GlobalSize size, GlobalCopy copy, string operation)
    {
        Check(size(out ulong byteCount), operation);
        if (byteCount == 0)
        {
            return string.Empty;
        }

        byte[] buffer = new byte[byteCount];
        fixed (byte* pointer = buffer)
        {
            Check(copy(pointer, byteCount, out ulong written), operation);
            return Encoding.UTF8.GetString(buffer, 0, (int)written);
        }
    }

    internal static string ReadString(
        NativeStringReader.SizeFunc size, NativeStringReader.CopyFunc copy, CnaHandle handle, string operation) =>
        NativeStringReader.Read(size, copy, handle, operation);

    internal delegate CnaResult BlobSize(CnaHandle handle, out ulong bytes);

    internal unsafe delegate CnaResult BlobCopy(CnaHandle handle, byte* destination, ulong capacity, out ulong bytes);

    internal static unsafe byte[] ReadBytes(BlobSize size, BlobCopy copy, CnaHandle handle, string operation)
    {
        Check(size(handle, out ulong byteCount), operation);
        byte[] buffer = new byte[byteCount];
        if (byteCount == 0)
        {
            return buffer;
        }

        fixed (byte* pointer = buffer)
        {
            Check(copy(handle, pointer, byteCount, out ulong written), operation);
            return written == byteCount ? buffer : buffer[..(int)written];
        }
    }

    internal static CnaResult WithString(string value, Func<CnaStringView, CnaResult> call) =>
        CnaStringMarshal.WithStringView(value ?? string.Empty, call);

    internal static byte Bool(bool value) => value ? (byte)1 : (byte)0;

    /// <summary>
    /// An exception thrown by game code a native callback ran -- an <c>AsyncCallback</c>, a
    /// <c>SignedIn</c> handler. It cannot unwind through native, so it goes to the running game,
    /// which stops at its next safe point and rethrows it from <c>Run</c>: where XNA's would have
    /// surfaced, out of the dispatcher update that ran the callback.
    /// </summary>
    internal static void ReportCallbackException(Exception exception)
    {
        if (CNA.Game.Active is { } game)
        {
            game.ReportComponentFailure(exception);
            return;
        }

        // No game to stop. The process-wide unhandled-exception path is what XNA would reach too.
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
    }
}

/// <summary>
/// XNA's Begin/End result. It completes either synchronously inside the Begin call or later from a
/// native completion callback on the game thread; either way the caller's <see cref="AsyncCallback"/>
/// runs once, with this result, after <see cref="IsCompleted"/> is set.
/// </summary>
internal sealed class GamerServicesAsyncResult : IAsyncResult
{
    private readonly AsyncCallback? _callback;
    private ManualResetEvent? _waitHandle;
    private GCHandle _selfHandle;
    private int _completed;

    internal GamerServicesAsyncResult(AsyncCallback? callback, object? asyncState, object owner)
    {
        _callback = callback;
        AsyncState = asyncState;
        Owner = owner;
    }

    /// <summary>The Begin call this result answers, so End can refuse a result from another one.</summary>
    internal object Owner { get; }

    /// <summary>A value the Begin side stored for End to return (a native handle, a reader).</summary>
    internal object? Payload { get; set; }

    internal bool Ended { get; set; }

    public object? AsyncState { get; }

    public bool CompletedSynchronously { get; private set; }

    public bool IsCompleted => Volatile.Read(ref _completed) != 0;

    public WaitHandle AsyncWaitHandle
    {
        get
        {
            if (_waitHandle is null)
            {
                var created = new ManualResetEvent(IsCompleted);
                if (Interlocked.CompareExchange(ref _waitHandle, created, null) is not null)
                {
                    created.Dispose();
                }
                else if (IsCompleted)
                {
                    _waitHandle.Set();
                }
            }

            return _waitHandle;
        }
    }

    /// <summary>The context native receives, keeping this result alive until it completes.</summary>
    internal nint NativeContext()
    {
        _selfHandle = GCHandle.Alloc(this);
        return GCHandle.ToIntPtr(_selfHandle);
    }

    /// <summary>The completion callback native calls; see <see cref="NativeContext"/>.</summary>
    internal static unsafe nint NativeCallback => (nint)(delegate* unmanaged[Cdecl]<nint, void>)&OnNativeCompletion;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnNativeCompletion(nint context)
    {
        if (context == 0)
        {
            return;
        }

        GCHandle handle = GCHandle.FromIntPtr(context);
        if (handle.Target is GamerServicesAsyncResult result)
        {
            result.Complete(synchronously: false);
        }
    }

    /// <summary>Marks the operation complete and runs the caller's callback exactly once.</summary>
    internal void Complete(bool synchronously)
    {
        if (Interlocked.Exchange(ref _completed, 1) != 0)
        {
            return;
        }

        CompletedSynchronously = synchronously;
        if (_selfHandle.IsAllocated)
        {
            _selfHandle.Free();
        }

        _waitHandle?.Set();
        if (_callback is null)
        {
            return;
        }

        try
        {
            _callback(this);
        }
        catch (Exception exception) when (!synchronously)
        {
            GamerServicesInterop.ReportCallbackException(exception);
        }
    }

    /// <summary>XNA's End checks: the right Begin, and only once.</summary>
    internal static GamerServicesAsyncResult ForEnd(IAsyncResult result, object owner, string parameterName = "result")
    {
        ArgumentNullException.ThrowIfNull(result, parameterName);
        if (result is not GamerServicesAsyncResult ours || !ReferenceEquals(ours.Owner, owner))
        {
            throw new ArgumentException("The IAsyncResult was not returned by the matching Begin call.", parameterName);
        }

        if (ours.Ended)
        {
            throw new InvalidOperationException("End has already been called for this IAsyncResult.");
        }

        ours.Ended = true;
        return ours;
    }
}
