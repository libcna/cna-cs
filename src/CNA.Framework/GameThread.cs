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
/// A game thread that blocks waiting for that worker (a <c>Join</c> in <c>Update</c>) deadlocks,
/// as it would under FNA's equivalent; XNA did not need the game thread for the load.
/// </summary>
internal static class GameThread
{
    // Time a frame spends on queued calls before it goes on to draw. A worker usually asks again as
    // soon as its previous call returns, so the drain waits for the next call within this budget.
    private static readonly long Budget = Stopwatch.Frequency * 8 / 1000;

    private static readonly object Gate = new();
    private static readonly Queue<Call> Pending = new();
    private static int _ownerThreadId;

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

    /// <summary>The calling thread created a game: from now on it is the game thread.</summary>
    internal static void Enter() => Volatile.Write(ref _ownerThreadId, Environment.CurrentManagedThreadId);

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
