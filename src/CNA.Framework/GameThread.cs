using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace CNA;

/// <summary>
/// Runs work for other threads on the game thread. CNA's handles belong to the thread that created
/// the game, and native refuses every other thread (<c>CNA_RESULT_THREAD</c>). XNA 4.0 let a game
/// load content on its own worker thread -- the loading-screen pattern -- so the XNA facade sends
/// such calls here: the caller blocks while the game thread runs the call at the start of its next
/// Update or Draw, and the frame goes on drawing the loading screen around it.
///
/// A game thread that blocks waiting for that worker -- XNA's loading screens join the thread that
/// draws their animation from <c>Update</c> -- runs the queued calls while it waits: the game
/// thread's <see cref="SynchronizationContext"/> asks to be told of its waits, which .NET does for
/// <c>Thread.Join</c>, wait handles, <c>Monitor.Wait</c> and <c>ManualResetEventSlim</c>, and runs
/// this queue and native CNA's (<c>cna_game_run_foreign_thread_calls_ext</c>) between short waits.
/// XNA ran the worker's calls concurrently; here they run while the game thread waits.
/// </summary>
internal static class GameThread
{
    // Time a frame spends on queued calls before it goes on to draw. A worker usually asks again as
    // soon as its previous call returns, so the drain waits for the next call within this budget.
    private static readonly long Budget = Stopwatch.Frequency * 8 / 1000;

    private static readonly object Gate = new();
    private static readonly Queue<Call> Pending = new();
    private static int _ownerThreadId;
    private static WaitingContext? _waitingContext;

    /// <summary>Whether the caller can use CNA's handles directly: it is the game thread, or no
    /// game exists to dispatch to.</summary>
    internal static bool IsCurrent
    {
        get
        {
            int owner = Volatile.Read(ref _ownerThreadId);
            return owner == 0 || owner == Environment.CurrentManagedThreadId;
        }
    }

    /// <summary>Runs <paramref name="work"/> on the game thread and returns its result, or rethrows
    /// what it threw.</summary>
    internal static T Invoke<T>(Func<T> work)
    {
        if (IsCurrent)
        {
            return work();
        }

        var call = new Call(() => work());
        lock (Gate)
        {
            // The game ended since IsCurrent was read: nothing will run the queue any more.
            if (_ownerThreadId == 0)
            {
                return work();
            }

            Pending.Enqueue(call);
            Monitor.PulseAll(Gate);
        }

        call.Done.Wait();
        call.Done.Dispose();
        call.Failure?.Throw();
        return (T)call.Result!;
    }

    /// <summary>Runs <paramref name="work"/> on the game thread, or rethrows what it threw.</summary>
    internal static void Invoke(Action work) => Invoke<object?>(() =>
    {
        work();
        return null;
    });

    /// <summary>Runs <paramref name="work"/> with <paramref name="state"/> on the game thread. A
    /// static lambda and a state tuple keep the caller's game-thread path free of a closure
    /// allocation, which capturing parameters would cost on every call.</summary>
    internal static void Invoke<TState>(Action<TState> work, TState state) => Invoke<object?>(() =>
    {
        work(state);
        return null;
    });

    /// <summary>The value-returning form of <see cref="Invoke{TState}(Action{TState}, TState)"/>.</summary>
    internal static TResult Invoke<TState, TResult>(Func<TState, TResult> work, TState state) =>
        Invoke(() => work(state));

    /// <summary>The calling thread created a game: from now on it is the game thread. Its waits run
    /// the queued calls, <paramref name="runNativeCalls"/> included. Not in a browser, whose
    /// runtime owns the game thread's synchronization context.</summary>
    internal static void Enter(Action runNativeCalls)
    {
        Volatile.Write(ref _ownerThreadId, Environment.CurrentManagedThreadId);
        if (!OperatingSystem.IsBrowser())
        {
            _waitingContext = new WaitingContext(SynchronizationContext.Current, runNativeCalls);
            SynchronizationContext.SetSynchronizationContext(_waitingContext);
        }
    }

    /// <summary>The game thread's game is gone: calls still waiting fail instead of waiting for a
    /// frame that will not come.</summary>
    internal static void Exit()
    {
        lock (Gate)
        {
            if (_ownerThreadId != Environment.CurrentManagedThreadId)
            {
                return;
            }

            Volatile.Write(ref _ownerThreadId, 0);
            if (_waitingContext is { } waiting)
            {
                if (ReferenceEquals(SynchronizationContext.Current, waiting))
                {
                    SynchronizationContext.SetSynchronizationContext(waiting.Inner);
                }

                _waitingContext = null;
            }

            while (Pending.TryDequeue(out Call? call))
            {
                call.Fail(new InvalidOperationException("The game ended before the game thread could run this call."));
            }
        }
    }

    /// <summary>Runs the calls other threads queued. Called by the game thread at the start of each
    /// Update and Draw.</summary>
    internal static void RunPending()
    {
        long deadline = 0;
        while (true)
        {
            Call call;
            lock (Gate)
            {
                if (Pending.Count == 0)
                {
                    if (deadline == 0)
                    {
                        return;
                    }

                    long remaining = deadline - Stopwatch.GetTimestamp();
                    if (remaining <= 0
                        || !Monitor.Wait(Gate, TimeSpan.FromSeconds((double)remaining / Stopwatch.Frequency))
                        || Pending.Count == 0)
                    {
                        return;
                    }
                }

                call = Pending.Dequeue();
            }

            if (deadline == 0)
            {
                deadline = Stopwatch.GetTimestamp() + Budget;
            }

            call.Run();
        }
    }

    /// <summary>The game thread's synchronization context: it posts and sends as the context it
    /// replaced did, and runs the queued calls between short slices of every wait.</summary>
    private sealed class WaitingContext : SynchronizationContext
    {
        private const int Slice = 2;
        private const int WaitTimeout = 0x102;

        [ThreadStatic]
        private static bool t_running;

        private readonly Action _runNativeCalls;

        public WaitingContext(SynchronizationContext? inner, Action runNativeCalls)
        {
            Inner = inner;
            _runNativeCalls = runNativeCalls;
            SetWaitNotificationRequired();
        }

        public SynchronizationContext? Inner { get; }

        public override void Post(SendOrPostCallback d, object? state)
        {
            if (Inner is not null)
            {
                Inner.Post(d, state);
            }
            else
            {
                base.Post(d, state);
            }
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            if (Inner is not null)
            {
                Inner.Send(d, state);
            }
            else
            {
                base.Send(d, state);
            }
        }

        public override SynchronizationContext CreateCopy() => this;

        public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            // A call that waits while it runs waits plainly: the queue is already being run.
            if (t_running || Environment.CurrentManagedThreadId != Volatile.Read(ref _ownerThreadId))
            {
                return WaitHelper(waitHandles, waitAll, millisecondsTimeout);
            }

            bool forever = millisecondsTimeout == Timeout.Infinite;
            long deadline = Environment.TickCount64 + (forever ? 0 : millisecondsTimeout);
            while (true)
            {
                t_running = true;
                try
                {
                    RunPending();
                    _runNativeCalls();
                }
                finally
                {
                    t_running = false;
                }

                int slice = forever ? Slice : (int)Math.Clamp(deadline - Environment.TickCount64, 0, Slice);
                int result = WaitHelper(waitHandles, waitAll, slice);
                if (result != WaitTimeout || (!forever && Environment.TickCount64 >= deadline))
                {
                    return result;
                }
            }
        }
    }

    private sealed class Call(Func<object?> work)
    {
        public ManualResetEventSlim Done { get; } = new();

        public object? Result { get; private set; }

        public ExceptionDispatchInfo? Failure { get; private set; }

        public void Run()
        {
            try
            {
                Result = work();
            }
            catch (Exception ex)
            {
                Failure = ExceptionDispatchInfo.Capture(ex);
            }

            Done.Set();
        }

        public void Fail(Exception ex)
        {
            Failure = ExceptionDispatchInfo.Capture(ex);
            Done.Set();
        }
    }
}
